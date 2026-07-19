namespace WFFramework.Gameplay.Tasks
{
    /// <summary>
    /// Trạng thái lifecycle của một Task.
    /// </summary>
    public enum GameTaskStatus
    {
        Pending,
        Assigned,
        Running,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Interface chung của một Task.
    /// </summary>
    public interface IGameTask<TAgent>
    {
        /// <summary>
        /// ID duy nhất của Task.
        /// </summary>
        string TaskId { get; }

        /// <summary>
        /// Priority cơ bản.
        ///
        /// Priority càng cao thì Task càng được ưu tiên.
        /// </summary>
        int Priority { get; }

        GameTaskStatus Status { get; }

        /// <summary>
        /// Kiểm tra Agent có đủ điều kiện nhận Task không.
        /// </summary>
        bool CanAssign(TAgent agent);

        /// <summary>
        /// Tính điểm phù hợp riêng của Task với Agent.
        ///
        /// Ví dụ:
        /// - Khoảng cách.
        /// - Kỹ năng.
        /// - Role.
        /// - Mức độ khẩn cấp.
        /// </summary>
        float CalculateScore(TAgent agent);

        /// <summary>
        /// Thử cấp Task cho Agent.
        /// </summary>
        bool TryAssign(TAgent agent);

        /// <summary>
        /// Bắt đầu thực thi Task.
        /// </summary>
        void Begin(TAgent agent);

        /// <summary>
        /// Cập nhật Task trong lúc đang chạy.
        /// </summary>
        void Tick(
            TAgent agent,
            float deltaTime);

        /// <summary>
        /// Hủy Task.
        /// </summary>
        void Cancel(TAgent agent);
    }
}