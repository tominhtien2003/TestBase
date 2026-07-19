using System;

namespace WFFramework.Core.StateMachine
{
    /// <summary>
    /// Generic State Machine hỗ trợ pending transition.
    ///
    /// Pending transition giúp tránh đổi State trực tiếp trong lúc
    /// State hiện tại đang chạy Tick.
    ///
    /// Có thể dùng cho:
    /// - Bot.
    /// - Patient.
    /// - Monster.
    /// - Ghost.
    /// - Player mode.
    /// - UI flow.
    /// - Game state.
    /// </summary>
    public sealed class StateMachine<TContext>
    {
        /// <summary>
        /// Context dùng chung cho toàn bộ State.
        /// </summary>
        private readonly TContext _context;

        /// <summary>
        /// State đang hoạt động.
        /// </summary>
        public IState<TContext> CurrentState { get; private set; }

        /// <summary>
        /// State đang chờ được chuyển tới.
        ///
        /// State này sẽ được áp dụng trước hoặc sau Tick.
        /// </summary>
        private IState<TContext> _pendingState;

        /// <summary>
        /// True khi StateMachine đang thực hiện transition.
        /// </summary>
        private bool _isTransitioning;

        /// <summary>
        /// Khởi tạo StateMachine với Context cố định.
        /// </summary>
        public StateMachine(TContext context)
        {
            _context = context ??
                       throw new ArgumentNullException(
                           nameof(context));
        }

        /// <summary>
        /// Bắt đầu StateMachine với Initial State.
        ///
        /// Thường chỉ gọi một lần khi Controller Initialize.
        /// </summary>
        public void Start(IState<TContext> initialState)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(
                    nameof(initialState));
            }

            if (CurrentState != null)
            {
                throw new InvalidOperationException(
                    "StateMachine đã được Start.");
            }

            _pendingState = initialState;
            ApplyPendingTransition();
        }

        /// <summary>
        /// Yêu cầu chuyển State an toàn.
        ///
        /// State không bị đổi ngay lập tức nếu đang Tick.
        /// </summary>
        public void RequestStateChange(
            IState<TContext> newState)
        {
            if (newState == null)
            {
                throw new ArgumentNullException(
                    nameof(newState));
            }

            if (ReferenceEquals(CurrentState, newState))
            {
                return;
            }

            _pendingState = newState;
        }

        /// <summary>
        /// Tick State hiện tại.
        ///
        /// Pending State được áp dụng cả trước và sau Tick.
        /// </summary>
        public void Tick(float deltaTime)
        {
            ApplyPendingTransition();

            CurrentState?.Tick(
                _context,
                deltaTime);

            ApplyPendingTransition();
        }

        /// <summary>
        /// Dừng StateMachine và Exit State hiện tại.
        /// </summary>
        public void Stop()
        {
            _pendingState = null;

            if (CurrentState == null)
            {
                return;
            }

            CurrentState.Exit(_context);
            CurrentState = null;
        }

        /// <summary>
        /// Thực hiện transition đang chờ.
        /// </summary>
        private void ApplyPendingTransition()
        {
            if (_pendingState == null || _isTransitioning)
            {
                return;
            }

            _isTransitioning = true;

            IState<TContext> nextState = _pendingState;
            _pendingState = null;

            CurrentState?.Exit(_context);

            CurrentState = nextState;
            CurrentState.Enter(_context);

            _isTransitioning = false;
        }
    }
}