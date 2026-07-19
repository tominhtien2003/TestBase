using System;

namespace WFFramework.Gameplay.Tasks
{
    /// <summary>
    /// Base class quản lý lifecycle chung của Task.
    ///
    /// Game cụ thể chỉ cần implement:
    /// - CanAssignInternal.
    /// - CalculateScoreInternal.
    /// - OnBegin.
    /// - OnTick.
    /// - OnCancel.
    /// </summary>
    public abstract class GameTaskBase<TAgent> :
        IGameTask<TAgent>
    {
        public string TaskId { get; }

        public int Priority { get; }

        public GameTaskStatus Status { get; private set; }

        protected GameTaskBase(
            string taskId,
            int priority)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                throw new ArgumentException(
                    "TaskId không được để trống.",
                    nameof(taskId));
            }

            TaskId = taskId;
            Priority = priority;
            Status = GameTaskStatus.Pending;
        }

        /// <summary>
        /// Kiểm tra Task đang Pending và Agent hợp lệ.
        /// </summary>
        public bool CanAssign(TAgent agent)
        {
            return Status == GameTaskStatus.Pending &&
                   CanAssignInternal(agent);
        }

        /// <summary>
        /// Tính điểm cuối cùng:
        ///
        /// Priority + điểm riêng của Task.
        /// </summary>
        public float CalculateScore(TAgent agent)
        {
            return Priority +
                   CalculateScoreInternal(agent);
        }

        /// <summary>
        /// Chuyển Task từ Pending sang Assigned.
        /// </summary>
        public bool TryAssign(TAgent agent)
        {
            if (!CanAssign(agent))
            {
                return false;
            }

            if (!OnTryAssign(agent))
            {
                return false;
            }

            Status = GameTaskStatus.Assigned;
            return true;
        }

        /// <summary>
        /// Chuyển Task từ Assigned sang Running.
        /// </summary>
        public void Begin(TAgent agent)
        {
            if (Status != GameTaskStatus.Assigned)
            {
                throw new InvalidOperationException(
                    $"Task {TaskId} chưa được Assign.");
            }

            Status = GameTaskStatus.Running;
            OnBegin(agent);
        }

        /// <summary>
        /// Tick Task khi đang Running.
        /// </summary>
        public void Tick(
            TAgent agent,
            float deltaTime)
        {
            if (Status != GameTaskStatus.Running)
            {
                return;
            }

            OnTick(agent, deltaTime);
        }

        /// <summary>
        /// Hủy Task nếu chưa kết thúc.
        /// </summary>
        public void Cancel(TAgent agent)
        {
            if (IsFinished())
            {
                return;
            }

            OnCancel(agent);
            Status = GameTaskStatus.Cancelled;
        }

        /// <summary>
        /// Đánh dấu Task hoàn thành.
        ///
        /// Class kế thừa gọi hàm này khi công việc thành công.
        /// </summary>
        protected void Complete()
        {
            if (Status == GameTaskStatus.Running)
            {
                Status = GameTaskStatus.Completed;
            }
        }

        /// <summary>
        /// Đánh dấu Task thất bại.
        /// </summary>
        protected void Fail()
        {
            if (!IsFinished())
            {
                Status = GameTaskStatus.Failed;
            }
        }

        /// <summary>
        /// Kiểm tra Task đã vào trạng thái kết thúc hay chưa.
        /// </summary>
        private bool IsFinished()
        {
            return Status == GameTaskStatus.Completed ||
                   Status == GameTaskStatus.Failed ||
                   Status == GameTaskStatus.Cancelled;
        }

        protected abstract bool CanAssignInternal(
            TAgent agent);

        protected virtual float CalculateScoreInternal(
            TAgent agent)
        {
            return 0f;
        }

        /// <summary>
        /// Có thể override để Reserve Resource trong lúc Assign.
        /// </summary>
        protected virtual bool OnTryAssign(TAgent agent)
        {
            return true;
        }

        protected abstract void OnBegin(TAgent agent);

        protected abstract void OnTick(
            TAgent agent,
            float deltaTime);

        protected virtual void OnCancel(TAgent agent)
        {
        }
    }
}