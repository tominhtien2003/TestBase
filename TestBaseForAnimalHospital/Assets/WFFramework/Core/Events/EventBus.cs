using System;
using System.Collections.Generic;
using UnityEngine;
using WFFramework.Core.Services;
using WFFramework.Core.Utilities;


namespace WFFramework.Core.Events
{
    /// <summary>
    /// Event Bus đồng bộ, generic và không dùng static event.
    ///
    /// Event được xử lý ngay trong lúc Publish.
    /// Không nên dùng EventBus cho dữ liệu cần xử lý bất đồng bộ.
    /// </summary>
    public sealed class EventBus : IEventBus, IGameSystem
    {
        /// <summary>
        /// Dictionary lưu listener theo loại event.
        ///
        /// Ví dụ:
        /// typeof(PatientDiedEvent) → Action&lt;PatientDiedEvent&gt;.
        /// </summary>
        private readonly Dictionary<Type, Delegate> _listeners =
            new Dictionary<Type, Delegate>();

        /// <summary>
        /// EventBus hiện không cần dependency khác.
        /// Hàm vẫn tồn tại để tuân theo lifecycle chung.
        /// </summary>
        public void Initialize(GameContext context)
        {
        }

        public void StartGame()
        {
            
        }

        /// <summary>
        /// Đăng ký listener và trả về handle dùng để unsubscribe.
        /// </summary>
        public IDisposable Subscribe<TEvent>(
            Action<TEvent> listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            Type eventType = typeof(TEvent);

            if (_listeners.TryGetValue(
                    eventType,
                    out Delegate current))
            {
                _listeners[eventType] =
                    Delegate.Combine(current, listener);
            }
            else
            {
                _listeners.Add(eventType, listener);
            }

            // Khi Dispose handle, listener sẽ tự được gỡ.
            return new DisposableAction(
                () => Unsubscribe(listener));
        }

        /// <summary>
        /// Gửi event đến từng listener.
        ///
        /// Mỗi listener được bọc try/catch để lỗi ở một listener
        /// không ngăn các listener còn lại nhận event.
        /// </summary>
        public void Publish<TEvent>(TEvent eventData)
        {
            if (!_listeners.TryGetValue(
                    typeof(TEvent),
                    out Delegate current))
            {
                return;
            }

            Delegate[] invocationList =
                current.GetInvocationList();

            for (int i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    if (invocationList[i] is Action<TEvent> callback)
                    {
                        callback.Invoke(eventData);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        /// <summary>
        /// Xóa toàn bộ listener.
        /// </summary>
        public void Clear()
        {
            _listeners.Clear();
        }

        /// <summary>
        /// Release EventBus khi game kết thúc.
        /// </summary>
        public void Release()
        {
            Clear();
        }
        /// <summary>
        /// Gỡ một listener khỏi EventBus.
        ///
        /// Hàm private vì listener nên được gỡ thông qua IDisposable.
        /// </summary>
        private void Unsubscribe<TEvent>(
            Action<TEvent> listener)
        {
            Type eventType = typeof(TEvent);

            if (!_listeners.TryGetValue(
                    eventType,
                    out Delegate current))
            {
                return;
            }

            Delegate result =
                Delegate.Remove(current, listener);

            if (result == null)
            {
                _listeners.Remove(eventType);
            }
            else
            {
                _listeners[eventType] = result;
            }
        }
    }
}