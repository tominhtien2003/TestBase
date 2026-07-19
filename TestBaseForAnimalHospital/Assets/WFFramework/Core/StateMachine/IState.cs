namespace WFFramework.Core.StateMachine
{
    /// <summary>
    /// Interface cơ bản của một State.
    ///
    /// TContext chứa toàn bộ dependency mà State cần.
    /// State không nên tự gọi GetComponent hoặc FindObjectOfType.
    /// </summary>
    public interface IState<TContext>
    {
        /// <summary>
        /// Được gọi một lần khi bắt đầu State.
        /// </summary>
        void Enter(TContext context);

        /// <summary>
        /// Được gọi trong lúc State đang active.
        /// </summary>
        void Tick(
            TContext context,
            float deltaTime);

        /// <summary>
        /// Được gọi một lần trước khi rời State.
        /// </summary>
        void Exit(TContext context);
    }
}