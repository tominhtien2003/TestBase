namespace WFFramework.Core.StateMachine
{
    /// <summary>
    /// Base class cho một State có StateMachine con.
    ///
    /// Đây là nền tảng của HFSM.
    ///
    /// Ví dụ:
    /// AliveState
    /// ├── ReceptionFlow
    /// ├── ExaminationFlow
    /// └── LeavingFlow
    /// </summary>
    public abstract class HierarchicalState<TContext> :
        IState<TContext>
    {
        /// <summary>
        /// StateMachine con được quản lý bởi State cha.
        /// </summary>
        protected StateMachine<TContext> ChildMachine { get; }

        /// <summary>
        /// Khởi tạo State cha và ChildMachine dùng chung Context.
        /// </summary>
        protected HierarchicalState(TContext context)
        {
            ChildMachine =
                new StateMachine<TContext>(context);
        }

        /// <summary>
        /// Được gọi khi State cha bắt đầu.
        ///
        /// Sau OnEnter, HFSM sẽ lấy Initial Child State.
        /// </summary>
        public void Enter(TContext context)
        {
            OnEnter(context);

            IState<TContext> initialChildState =
                GetInitialChildState(context);

            if (initialChildState != null)
            {
                ChildMachine.Start(initialChildState);
            }
        }

        /// <summary>
        /// Tick State cha trước.
        ///
        /// Nếu OnTick trả về true, ChildMachine mới được Tick.
        /// State cha có thể trả về false khi đã yêu cầu chuyển Root State.
        /// </summary>
        public void Tick(
            TContext context,
            float deltaTime)
        {
            bool shouldTickChild =
                OnTick(context, deltaTime);

            if (shouldTickChild)
            {
                ChildMachine.Tick(deltaTime);
            }
        }

        /// <summary>
        /// Dừng ChildMachine trước, sau đó Exit State cha.
        /// </summary>
        public void Exit(TContext context)
        {
            ChildMachine.Stop();
            OnExit(context);
        }

        /// <summary>
        /// Trả về State con đầu tiên khi State cha bắt đầu.
        ///
        /// Có thể trả về null nếu State cha chưa cần State con.
        /// </summary>
        protected abstract IState<TContext>
            GetInitialChildState(TContext context);

        /// <summary>
        /// Logic chạy một lần khi vào State cha.
        /// </summary>
        protected virtual void OnEnter(TContext context)
        {
        }

        /// <summary>
        /// Logic chạy mỗi Tick ở cấp State cha.
        ///
        /// Trả về false nếu không muốn Tick State con trong Tick hiện tại.
        /// </summary>
        protected virtual bool OnTick(
            TContext context,
            float deltaTime)
        {
            return true;
        }

        /// <summary>
        /// Logic chạy một lần khi thoát State cha.
        /// </summary>
        protected virtual void OnExit(TContext context)
        {
        }
    }
}