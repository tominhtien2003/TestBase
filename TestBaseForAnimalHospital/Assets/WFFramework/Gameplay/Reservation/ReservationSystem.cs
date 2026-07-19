using System;
using System.Collections.Generic;
using WFFramework.Core.Services;

namespace WFFramework.Gameplay.Reservation
{
    /// <summary>
    /// Implementation mặc định của Reservation System.
    ///
    /// Mỗi Resource chỉ có thể có một Owner tại cùng một thời điểm.
    /// </summary>
    public sealed class ReservationSystem : IReservationService, IGameSystem
    {
        /// <summary>
        /// Lưu Reservation theo ResourceId.
        ///
        /// ResourceId → ReservationRecord.
        /// </summary>
        private readonly Dictionary<string, ReservationRecord>
            _reservationsByResource =
                new Dictionary<string, ReservationRecord>();

        /// <summary>
        /// Lưu danh sách Resource mà mỗi Owner đang giữ.
        ///
        /// OwnerId → danh sách ResourceId.
        ///
        /// Dictionary này giúp ReleaseAll hoạt động nhanh,
        /// không cần duyệt toàn bộ Reservation.
        /// </summary>
        private readonly Dictionary<string, HashSet<string>>
            _resourcesByOwner =
                new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// ReservationService không cần dependency khác khi Initialize.
        /// </summary>
        public void Initialize(GameContext context)
        {
        }

        public void StartGame()
        {
            
        }

        /// <summary>
        /// Thử giữ quyền sử dụng một Resource.
        /// </summary>
        public bool TryAcquire(
            string resourceId,
            string ownerId,
            out IReservationLease lease)
        {
            lease = null;

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentException(
                    "ResourceId không được để trống.",
                    nameof(resourceId));
            }

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                throw new ArgumentException(
                    "OwnerId không được để trống.",
                    nameof(ownerId));
            }

            // Resource đã có người giữ nên không thể Acquire.
            if (_reservationsByResource.ContainsKey(resourceId))
            {
                return false;
            }

            // Token giúp đảm bảo chỉ đúng lease đã tạo
            // mới có quyền giải phóng Reservation.
            string token = Guid.NewGuid().ToString("N");

            ReservationLease reservationLease =
                new ReservationLease(
                    resourceId,
                    ownerId,
                    token,
                    ReleaseLease);

            ReservationRecord record =
                new ReservationRecord(
                    resourceId,
                    ownerId,
                    token,
                    reservationLease);

            _reservationsByResource.Add(resourceId, record);

            if (!_resourcesByOwner.TryGetValue(
                    ownerId,
                    out HashSet<string> resourceIds))
            {
                resourceIds = new HashSet<string>();
                _resourcesByOwner.Add(ownerId, resourceIds);
            }

            resourceIds.Add(resourceId);

            lease = reservationLease;
            return true;
        }

        /// <summary>
        /// Kiểm tra Resource đã bị giữ hay chưa.
        /// </summary>
        public bool IsReserved(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                return false;
            }

            return _reservationsByResource.ContainsKey(resourceId);
        }

        /// <summary>
        /// Kiểm tra Resource có thuộc Owner cụ thể không.
        /// </summary>
        public bool IsReservedBy(
            string resourceId,
            string ownerId)
        {
            if (!_reservationsByResource.TryGetValue(
                    resourceId,
                    out ReservationRecord record))
            {
                return false;
            }

            return record.OwnerId == ownerId;
        }

        /// <summary>
        /// Giải phóng toàn bộ Resource của một Owner.
        /// </summary>
        public void ReleaseAll(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return;
            }

            if (!_resourcesByOwner.TryGetValue(
                    ownerId,
                    out HashSet<string> resourceIds))
            {
                return;
            }

            // Copy ra array vì quá trình Release sẽ sửa HashSet gốc.
            string[] resourceIdArray =
                new string[resourceIds.Count];

            resourceIds.CopyTo(resourceIdArray);

            for (int i = 0; i < resourceIdArray.Length; i++)
            {
                string resourceId = resourceIdArray[i];

                if (!_reservationsByResource.TryGetValue(
                        resourceId,
                        out ReservationRecord record))
                {
                    continue;
                }

                RemoveReservation(record);
            }
        }

        /// <summary>
        /// Được ReservationLease gọi khi lease bị Dispose.
        /// </summary>
        private void ReleaseLease(ReservationLease lease)
        {
            if (lease == null)
            {
                return;
            }

            if (!_reservationsByResource.TryGetValue(
                    lease.ResourceId,
                    out ReservationRecord record))
            {
                lease.MarkReleased();
                return;
            }

            // Kiểm tra Token để tránh một lease cũ
            // vô tình giải phóng Reservation mới.
            if (record.Token != lease.Token)
            {
                lease.MarkReleased();
                return;
            }

            RemoveReservation(record);
        }

        /// <summary>
        /// Xóa một Reservation khỏi cả hai Dictionary.
        /// </summary>
        private void RemoveReservation(
            ReservationRecord record)
        {
            _reservationsByResource.Remove(record.ResourceId);

            if (_resourcesByOwner.TryGetValue(
                    record.OwnerId,
                    out HashSet<string> resourceIds))
            {
                resourceIds.Remove(record.ResourceId);

                if (resourceIds.Count == 0)
                {
                    _resourcesByOwner.Remove(record.OwnerId);
                }
            }

            record.Lease.MarkReleased();
        }

        /// <summary>
        /// Giải phóng toàn bộ Reservation khi game kết thúc.
        /// </summary>
        public void Release()
        {
            foreach (ReservationRecord record
                     in _reservationsByResource.Values)
            {
                record.Lease.MarkReleased();
            }

            _reservationsByResource.Clear();
            _resourcesByOwner.Clear();
        }

        /// <summary>
        /// Dữ liệu nội bộ của một Reservation.
        /// </summary>
        private sealed class ReservationRecord
        {
            public readonly string ResourceId;
            public readonly string OwnerId;
            public readonly string Token;
            public readonly ReservationLease Lease;

            public ReservationRecord(
                string resourceId,
                string ownerId,
                string token,
                ReservationLease lease)
            {
                ResourceId = resourceId;
                OwnerId = ownerId;
                Token = token;
                Lease = lease;
            }
        }

        /// <summary>
        /// Implementation của IReservationLease.
        ///
        /// Class này chỉ được tạo bởi ReservationService.
        /// </summary>
        private sealed class ReservationLease :
            IReservationLease
        {
            /// <summary>
            /// Callback yêu cầu ReservationService giải phóng lease.
            /// </summary>
            private Action<ReservationLease> _releaseAction;

            public string ResourceId { get; }
            public string OwnerId { get; }

            /// <summary>
            /// Token nội bộ dùng để xác minh lease.
            /// </summary>
            public string Token { get; }

            public bool IsReleased { get; private set; }

            public ReservationLease(
                string resourceId,
                string ownerId,
                string token,
                Action<ReservationLease> releaseAction)
            {
                ResourceId = resourceId;
                OwnerId = ownerId;
                Token = token;
                _releaseAction = releaseAction;
            }

            /// <summary>
            /// Yêu cầu Service giải phóng Resource.
            ///
            /// Dispose nhiều lần vẫn an toàn.
            /// </summary>
            public void Dispose()
            {
                if (IsReleased)
                {
                    return;
                }

                Action<ReservationLease> action =
                    _releaseAction;

                action?.Invoke(this);
            }

            /// <summary>
            /// Đánh dấu lease đã được giải phóng.
            ///
            /// Chỉ ReservationService được gọi hàm này.
            /// </summary>
            public void MarkReleased()
            {
                IsReleased = true;
                _releaseAction = null;
            }
        }
    }
}