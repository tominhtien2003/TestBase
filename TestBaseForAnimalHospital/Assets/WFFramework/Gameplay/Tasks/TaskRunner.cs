namespace WFFramework.Gameplay.Tasks
{
    /// <summary>
    /// Điều khiển Agent nhận và thực hiện một Task tại một thời điểm.
    ///
    /// TaskRunner không biết Agent là Bot, Patient hay Monster.
    /// </summary>
    public sealed class TaskRunner<TAgent>
    {
        /// <summary>
        /// Agent đang sở hữu Runner.
        /// </summary>
        private readonly TAgent _agent;

        /// <summary>
        /// Board cung cấp Task.
        /// </summary>
        private readonly TaskBoard<TAgent> _taskBoard;

        /// <summary>
        /// Task hiện tại.
        ///
        /// Null nghĩa là Agent đang rảnh.
        /// </summary>
        public IGameTask<TAgent> CurrentTask {
            get;
            private set;
        }

        public bool HasTask => CurrentTask != null;

        public TaskRunner(
            TAgent agent,
            TaskBoard<TAgent> taskBoard)
        {
            _agent = agent;
            _taskBoard = taskBoard;
        }

        /// <summary>
        /// Tick Task hiện tại hoặc tìm Task mới.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (CurrentTask == null)
            {
                TryFindTask();
                return;
            }

            CurrentTask.Tick(
                _agent,
                deltaTime);

            if (!IsCurrentTaskFinished())
            {
                return;
            }

            _taskBoard.Remove(CurrentTask);
            CurrentTask = null;
        }

        /// <summary>
        /// Hủy Task hiện tại.
        /// </summary>
        public void CancelCurrentTask()
        {
            if (CurrentTask == null)
            {
                return;
            }

            CurrentTask.Cancel(_agent);
            _taskBoard.Remove(CurrentTask);

            CurrentTask = null;
        }

        /// <summary>
        /// Tìm và bắt đầu Task phù hợp nhất.
        /// </summary>
        private void TryFindTask()
        {
            IGameTask<TAgent> task =
                _taskBoard.FindBestTask(_agent);

            if (task == null)
            {
                return;
            }

            if (!task.TryAssign(_agent))
            {
                return;
            }

            CurrentTask = task;
            CurrentTask.Begin(_agent);
        }

        /// <summary>
        /// Kiểm tra Task hiện tại đã kết thúc hay chưa.
        /// </summary>
        private bool IsCurrentTaskFinished()
        {
            if (CurrentTask == null)
            {
                return true;
            }

            GameTaskStatus status =
                CurrentTask.Status;

            return status == GameTaskStatus.Completed ||
                   status == GameTaskStatus.Failed ||
                   status == GameTaskStatus.Cancelled;
        }
    }
}