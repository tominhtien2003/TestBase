using System;

namespace WFFramework.Gameplay.Reservation
{
    /// <summary>
    /// Đại diện cho quyền sử dụng tạm thời một Resource.
    ///
    /// Ví dụ:
    /// - Bot giữ quyền sử dụng một chiếc ghế.
    /// - Patient giữ chỗ trong phòng khám.
    /// - Task giữ quyền sử dụng một item.
    ///
    /// Khi Dispose lease, Resource sẽ được giải phóng.
    /// </summary>
    public interface IReservationLease : IDisposable
    {
        /// <summary>
        /// ID của Resource đang được giữ.
        /// </summary>
        string ResourceId { get; }

        /// <summary>
        /// ID của object hoặc task đang giữ Resource.
        /// </summary>
        string OwnerId { get; }

        /// <summary>
        /// True nếu quyền giữ Resource đã được giải phóng.
        /// </summary>
        bool IsReleased { get; }
    }

    /// <summary>
    /// Service quản lý quyền sử dụng Resource trong gameplay.
    ///
    /// Service này không biết Resource là ghế, phòng, item hay waypoint.
    /// Nó chỉ làm việc với ResourceId và OwnerId.
    /// </summary>
    public interface IReservationService
    {
        /// <summary>
        /// Thử giữ quyền sử dụng một Resource.
        ///
        /// Trả về true nếu Resource chưa có Owner.
        /// Khi thành công, caller phải giữ lại lease và Dispose khi hoàn thành.
        /// </summary>
        bool TryAcquire(
            string resourceId,
            string ownerId,
            out IReservationLease lease);

        /// <summary>
        /// Kiểm tra Resource đã bị giữ hay chưa.
        /// </summary>
        bool IsReserved(string resourceId);

        /// <summary>
        /// Kiểm tra Resource có đang bị giữ bởi Owner cụ thể hay không.
        /// </summary>
        bool IsReservedBy(
            string resourceId,
            string ownerId);

        /// <summary>
        /// Giải phóng toàn bộ Resource đang được một Owner giữ.
        ///
        /// Thường dùng khi:
        /// - Bot bị disable.
        /// - Task bị cancel.
        /// - Patient chết.
        /// - Scene kết thúc.
        /// </summary>
        void ReleaseAll(string ownerId);
    }
}