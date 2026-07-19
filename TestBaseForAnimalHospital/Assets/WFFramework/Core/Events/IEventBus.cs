using System;

namespace WFFramework.Core.Events
{
    /// <summary>
    /// Interface giao tiếp giữa các hệ thống thông qua event.
    ///
    /// Publisher không cần biết Subscriber là ai.
    ///
    /// Ví dụ:
    /// PatientSystem publish PatientDiedEvent.
    /// GhostSystem và UI cùng subscribe event đó.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Đăng ký lắng nghe một loại event.
        ///
        /// Trả về IDisposable để caller có thể unsubscribe an toàn.
        /// </summary>
        IDisposable Subscribe<TEvent>(
            Action<TEvent> listener);

        /// <summary>
        /// Gửi một event đến toàn bộ listener của loại event đó.
        /// </summary>
        void Publish<TEvent>(TEvent eventData);

        /// <summary>
        /// Xóa toàn bộ listener.
        /// </summary>
        void Clear();
    }
}