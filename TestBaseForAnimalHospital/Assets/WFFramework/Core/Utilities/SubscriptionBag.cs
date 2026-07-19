using System;
using System.Collections.Generic;

namespace WFFramework.Core.Utilities
{
    /// <summary>
    /// Nhóm nhiều IDisposable để có thể hủy cùng lúc.
    ///
    /// Thường dùng cho:
    /// - Event subscriptions.
    /// - Tick registrations.
    /// - Temporary bindings.
    /// </summary>
    public sealed class SubscriptionBag : IDisposable
    {
        /// <summary>
        /// Danh sách subscription đang được quản lý.
        /// </summary>
        private readonly List<IDisposable> _subscriptions =
            new List<IDisposable>();

        /// <summary>
        /// Thêm một subscription vào bag.
        /// </summary>
        public void Add(IDisposable subscription)
        {
            if (subscription == null)
            {
                return;
            }

            _subscriptions.Add(subscription);
        }

        /// <summary>
        /// Dispose toàn bộ subscription theo thứ tự ngược.
        /// </summary>
        public void Dispose()
        {
            for (int i = _subscriptions.Count - 1; i >= 0; i--)
            {
                _subscriptions[i]?.Dispose();
            }

            _subscriptions.Clear();
        }
    }
}