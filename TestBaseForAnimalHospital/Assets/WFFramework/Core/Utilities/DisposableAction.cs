using System;

namespace WFFramework.Core.Utilities
{
    /// <summary>
    /// IDisposable đơn giản, thực thi một Action khi Dispose.
    ///
    /// Được dùng cho:
    /// - Event subscription.
    /// - Tick registration.
    /// - Reservation lease.
    ///
    /// Dispose có thể gọi nhiều lần nhưng Action chỉ chạy một lần.
    /// </summary>
    public class DisposableAction : IDisposable
    {
        /// <summary>
        /// Action sẽ được gọi khi Dispose.
        ///
        /// Sau lần Dispose đầu tiên, biến được gán null.
        /// </summary>
        private Action _disposeAction;

        /// <summary>
        /// Khởi tạo Disposable với hành động cần thực hiện khi hủy.
        /// </summary>
        public DisposableAction(Action disposeAction)
        {
            _disposeAction = disposeAction ??
                             throw new ArgumentNullException(
                                 nameof(disposeAction));
        }

        /// <summary>
        /// Thực thi Action và đánh dấu Disposable đã được hủy.
        /// </summary>
        public void Dispose()
        {
            Action action = _disposeAction;
            _disposeAction = null;

            action?.Invoke();
        }
    }
}