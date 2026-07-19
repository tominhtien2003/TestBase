using System;
using System.Collections.Generic;
using UnityEngine;
using WFFramework.Core.Services;
using WFFramework.Core.Utilities;

namespace WFFramework.Core.Tick
{
    /// <summary>
    /// Scheduler điều khiển Tick cho gameplay.
    ///
    /// Mục đích:
    /// - Không để mọi Bot tự chạy AI trong Update.
    /// - Giảm số lần tính toán.
    /// - Chia logic thành nhiều tần suất khác nhau.
    ///
    /// Movement và Animation vẫn có thể chạy bằng Update của Unity.
    /// </summary>
    public sealed class TickScheduler : MonoBehaviour, ITickSystem, IGameSystem
    {
        /// <summary>
        /// Khoảng thời gian giữa hai Fast Tick.
        /// </summary>
        [SerializeField, Min(0.01f)]
        private float fastInterval = 0.1f;

        /// <summary>
        /// Khoảng thời gian giữa hai Normal Tick.
        /// </summary>
        [SerializeField, Min(0.01f)]
        private float normalInterval = 0.25f;

        /// <summary>
        /// Khoảng thời gian giữa hai Slow Tick.
        /// </summary>
        [SerializeField, Min(0.01f)]
        private float slowInterval = 1f;

        /// <summary>
        /// Các channel runtime được tạo khi Initialize.
        /// </summary>
        private readonly Dictionary<TickGroup, TickChannel> _channels =
            new Dictionary<TickGroup, TickChannel>();

        /// <summary>
        /// Các thao tác Add/Remove phát sinh trong lúc đang Tick.
        ///
        /// Không được sửa List Tickable trực tiếp khi đang foreach.
        /// </summary>
        private readonly List<PendingOperation> _pendingOperations =
            new List<PendingOperation>();

        /// <summary>
        /// True khi Scheduler đang gọi Tick trên các object.
        /// </summary>
        private bool _isTicking;

        /// <summary>
        /// True sau khi Scheduler đã được Initialize.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Tạo các Tick Channel.
        ///
        /// CoreInstaller phải Bind TickScheduler trước các service
        /// cần đăng ký Tick.
        /// </summary>
        public void Initialize(GameContext context)
        {
            _channels.Clear();

            _channels.Add(
                TickGroup.Fast,
                new TickChannel(fastInterval));

            _channels.Add(
                TickGroup.Normal,
                new TickChannel(normalInterval));

            _channels.Add(
                TickGroup.Slow,
                new TickChannel(slowInterval));

            _isInitialized = true;
        }

        public void StartGame()
        {
            
        }

        /// <summary>
        /// Đăng ký một ITickable vào TickGroup.
        ///
        /// Một object không nên đăng ký hai lần vào cùng nhóm.
        /// </summary>
        public IDisposable Register(TickGroup group, ITickable tickable)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    "TickScheduler chưa được Initialize.");
            }

            if (tickable == null)
            {
                throw new ArgumentNullException(nameof(tickable));
            }

            if (IsRegisteredOrPending(group, tickable))
            {
                throw new InvalidOperationException($"{tickable.GetType().Name} đã đăng ký vào {group}.");
            }

            if (_isTicking)
            {
                _pendingOperations.Add(new PendingOperation(true, group, tickable));
            }
            else
            {
                AddInternal(group, tickable);
            }

            return new DisposableAction(() => RequestUnregister(group, tickable));
        }

        /// <summary>
        /// Unity gọi mỗi frame.
        ///
        /// Hàm chỉ tích lũy thời gian và chạy các channel đến hạn.
        /// </summary>
        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            TickChannel fastChannel = _channels[TickGroup.Fast];

            TickChannel normalChannel = _channels[TickGroup.Normal];

            TickChannel slowChannel = _channels[TickGroup.Slow];

            UpdateChannel(fastChannel, deltaTime);
            UpdateChannel(normalChannel, deltaTime);
            UpdateChannel(slowChannel, deltaTime);

            ApplyPendingOperations();
        }

        /// <summary>
        /// Cập nhật timer của một channel.
        ///
        /// Khi timer đạt interval, toàn bộ Tickable trong channel được gọi.
        /// </summary>
        private void UpdateChannel(TickChannel channel, float frameDeltaTime)
        {
            channel.Timer += frameDeltaTime;

            if (channel.Timer < channel.Interval)
            {
                return;
            }

            // Dùng toàn bộ thời gian thực đã trôi qua,
            // giúp logic ổn định hơn khi frame bị chậm.
            float elapsedTime = channel.Timer;
            channel.Timer = 0f;

            _isTicking = true;

            for (int i = 0; i < channel.Tickables.Count; i++)
            {
                try
                {
                    channel.Tickables[i].Tick(elapsedTime);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            _isTicking = false;
        }

        /// <summary>
        /// Yêu cầu unregister một Tickable.
        ///
        /// Nếu đang Tick, thao tác sẽ được trì hoãn.
        /// </summary>
        private void RequestUnregister(TickGroup group, ITickable tickable)
        {
            if (!_isInitialized || tickable == null)
            {
                return;
            }

            if (_isTicking)
            {
                _pendingOperations.Add(new PendingOperation(false, group, tickable));

                return;
            }

            RemoveInternal(group, tickable);
        }

        /// <summary>
        /// Thêm Tickable trực tiếp vào channel.
        /// </summary>
        private void AddInternal(TickGroup group, ITickable tickable)
        {
            TickChannel channel = _channels[group];

            if (!channel.Tickables.Contains(tickable))
            {
                channel.Tickables.Add(tickable);
            }
        }

        /// <summary>
        /// Gỡ Tickable trực tiếp khỏi channel.
        /// </summary>
        private void RemoveInternal(TickGroup group, ITickable tickable)
        {
            if (!_channels.TryGetValue(group, out TickChannel channel))
            {
                return;
            }

            channel.Tickables.Remove(tickable);
        }

        /// <summary>
        /// Áp dụng các thao tác Add/Remove sau khi Tick hoàn thành.
        /// </summary>
        private void ApplyPendingOperations()
        {
            if (_pendingOperations.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _pendingOperations.Count; i++)
            {
                PendingOperation operation = _pendingOperations[i];

                if (operation.IsAdd)
                {
                    AddInternal(operation.Group, operation.Tickable);
                }
                else
                {
                    RemoveInternal(operation.Group, operation.Tickable);
                }
            }

            _pendingOperations.Clear();
        }

        /// <summary>
        /// Kiểm tra Tickable đã đăng ký hoặc đang chờ đăng ký chưa.
        /// </summary>
        private bool IsRegisteredOrPending(TickGroup group, ITickable tickable)
        {
            if (_channels[group].Tickables.Contains(tickable))
            {
                return true;
            }

            for (int i = 0; i < _pendingOperations.Count; i++)
            {
                PendingOperation operation = _pendingOperations[i];

                if (!operation.IsAdd)
                {
                    continue;
                }

                if (operation.Group == group && ReferenceEquals(operation.Tickable, tickable))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Xóa toàn bộ registration khi game kết thúc.
        /// </summary>
        public void Release()
        {
            foreach (TickChannel channel in _channels.Values)
            {
                channel.Tickables.Clear();
                channel.Timer = 0f;
            }

            _pendingOperations.Clear();
            _channels.Clear();

            _isTicking = false;
            _isInitialized = false;
        }

        /// <summary>
        /// Dữ liệu runtime của một TickGroup.
        /// </summary>
        private sealed class TickChannel
        {
            /// <summary>
            /// Thời gian giữa hai Tick.
            /// </summary>
            public readonly float Interval;

            /// <summary>
            /// Thời gian đã tích lũy từ lần Tick trước.
            /// </summary>
            public float Timer;

            /// <summary>
            /// Danh sách object nhận Tick.
            /// </summary>
            public readonly List<ITickable> Tickables = new List<ITickable>();

            public TickChannel(float interval)
            {
                Interval = interval;
            }
        }

        /// <summary>
        /// Đại diện cho thao tác Add hoặc Remove bị trì hoãn.
        /// </summary>
        private readonly struct PendingOperation
        {
            /// <summary>
            /// True là Add, false là Remove.
            /// </summary>
            public readonly bool IsAdd;

            /// <summary>
            /// TickGroup cần thay đổi.
            /// </summary>
            public readonly TickGroup Group;

            /// <summary>
            /// Object cần Add hoặc Remove.
            /// </summary>
            public readonly ITickable Tickable;

            public PendingOperation(bool isAdd, TickGroup group, ITickable tickable)
            {
                IsAdd = isAdd;
                Group = group;
                Tickable = tickable;
            }
        }
    }
}