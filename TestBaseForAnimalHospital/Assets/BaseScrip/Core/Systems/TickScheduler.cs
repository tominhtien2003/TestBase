using System;
using System.Collections.Generic;
using UnityEngine;

public enum TickGroup { Fast, Normal, Slow }

public interface ITickable
{
    void Tick(float deltaTime);
}

public interface ITickSystem
{
    IDisposable Register(TickGroup group, ITickable tickable);
}

[DefaultExecutionOrder(-100)]
public sealed class TickScheduler : MonoBehaviour, ITickSystem
{
    public static TickScheduler Instance { get; private set; }

    [SerializeField, Min(0.01f)] private float fastInterval = 0.1f;
    [SerializeField, Min(0.01f)] private float normalInterval = 0.25f;
    [SerializeField, Min(0.01f)] private float slowInterval = 1f;

    private readonly Dictionary<TickGroup, TickChannel> _channels = new Dictionary<TickGroup, TickChannel>();
    private readonly List<PendingOperation> _pendingOperations = new List<PendingOperation>();
    
    private bool _isTicking;
    private bool _isInitialized;

    // ==========================================
    // 2. CHUYỂN INITIALIZE THÀNH AWAKE CỦA UNITY
    // ==========================================
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _channels.Clear();
        _channels.Add(TickGroup.Fast, new TickChannel(fastInterval));
        _channels.Add(TickGroup.Normal, new TickChannel(normalInterval));
        _channels.Add(TickGroup.Slow, new TickChannel(slowInterval));

        _isInitialized = true;
    }

    public IDisposable Register(TickGroup group, ITickable tickable)
    {
        if (!_isInitialized) throw new InvalidOperationException("TickScheduler chưa Awake.");
        if (tickable == null) throw new ArgumentNullException(nameof(tickable));
        if (IsRegisteredOrPending(group, tickable)) throw new InvalidOperationException("Đã đăng ký rồi.");

        if (_isTicking)
        {
            _pendingOperations.Add(new PendingOperation(true, group, tickable));
        }
        else
        {
            AddInternal(group, tickable);
        }

        // Dùng class DisposableAction nội bộ bên dưới
        return new DisposableAction(() => RequestUnregister(group, tickable));
    }

    private void Update()
    {
        if (!_isInitialized) return;

        float deltaTime = Time.deltaTime;
        UpdateChannel(_channels[TickGroup.Fast], deltaTime);
        UpdateChannel(_channels[TickGroup.Normal], deltaTime);
        UpdateChannel(_channels[TickGroup.Slow], deltaTime);

        ApplyPendingOperations();
    }

    private void UpdateChannel(TickChannel channel, float frameDeltaTime)
    {
        channel.Timer += frameDeltaTime;
        if (channel.Timer < channel.Interval) return;

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

    private void RequestUnregister(TickGroup group, ITickable tickable)
    {
        if (!_isInitialized || tickable == null) return;

        if (_isTicking)
        {
            _pendingOperations.Add(new PendingOperation(false, group, tickable));
            return;
        }
        RemoveInternal(group, tickable);
    }

    private void AddInternal(TickGroup group, ITickable tickable)
    {
        TickChannel channel = _channels[group];
        if (!channel.Tickables.Contains(tickable)) channel.Tickables.Add(tickable);
    }

    private void RemoveInternal(TickGroup group, ITickable tickable)
    {
        if (_channels.TryGetValue(group, out TickChannel channel))
        {
            channel.Tickables.Remove(tickable);
        }
    }

    private void ApplyPendingOperations()
    {
        if (_pendingOperations.Count == 0) return;

        for (int i = 0; i < _pendingOperations.Count; i++)
        {
            PendingOperation operation = _pendingOperations[i];
            if (operation.IsAdd) AddInternal(operation.Group, operation.Tickable);
            else RemoveInternal(operation.Group, operation.Tickable);
        }
        _pendingOperations.Clear();
    }

    private bool IsRegisteredOrPending(TickGroup group, ITickable tickable)
    {
        if (_channels[group].Tickables.Contains(tickable)) return true;
        for (int i = 0; i < _pendingOperations.Count; i++)
        {
            PendingOperation operation = _pendingOperations[i];
            if (operation.IsAdd && operation.Group == group && ReferenceEquals(operation.Tickable, tickable)) return true;
        }
        return false;
    }

    // ==========================================
    // 3. CHUYỂN RELEASE THÀNH ONDESTROY CỦA UNITY
    // ==========================================
    private void OnDestroy()
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

    private sealed class TickChannel
    {
        public readonly float Interval;
        public float Timer;
        public readonly List<ITickable> Tickables = new List<ITickable>();

        public TickChannel(float interval) { Interval = interval; }
    }

    private readonly struct PendingOperation
    {
        public readonly bool IsAdd;
        public readonly TickGroup Group;
        public readonly ITickable Tickable;

        public PendingOperation(bool isAdd, TickGroup group, ITickable tickable)
        {
            IsAdd = isAdd;
            Group = group;
            Tickable = tickable;
        }
    }
}

// ==========================================
// 4. MANG THEO CÔNG CỤ DỌN RÁC (IDisposable)
// ==========================================
public class DisposableAction : IDisposable
{
    private Action _action;
    public DisposableAction(Action action) { _action = action; }
    public void Dispose()
    {
        _action?.Invoke();
        _action = null;
    }
}
