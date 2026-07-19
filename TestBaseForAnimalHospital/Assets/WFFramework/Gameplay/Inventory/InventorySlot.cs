using System;

namespace WFFramework.Gameplay.Inventory
{
    /// <summary>
    /// Một ô trong Inventory.
    ///
    /// Slot chỉ lưu ItemId và Quantity,
    /// không lưu trực tiếp ScriptableObject để dễ Save/Load.
    /// </summary>
    [Serializable]
    public sealed class InventorySlot
    {
        /// <summary>
        /// ID của Item trong Slot.
        ///
        /// Null hoặc rỗng nghĩa là Slot trống.
        /// </summary>
        private string _itemId;

        /// <summary>
        /// Số lượng Item đang có trong Slot.
        /// </summary>
        private int _quantity;

        public string ItemId => _itemId;
        public int Quantity => _quantity;

        /// <summary>
        /// True nếu Slot không chứa Item hợp lệ.
        /// </summary>
        public bool IsEmpty =>
            string.IsNullOrEmpty(_itemId) ||
            _quantity <= 0;

        /// <summary>
        /// Gán dữ liệu vào Slot.
        ///
        /// Chỉ InventoryModel nên gọi hàm này.
        /// </summary>
        internal void Set(
            string itemId,
            int quantity)
        {
            _itemId = itemId;
            _quantity = quantity;
        }

        /// <summary>
        /// Thêm số lượng vào Slot hiện tại.
        /// </summary>
        internal void Add(int quantity)
        {
            _quantity += quantity;
        }

        /// <summary>
        /// Trừ số lượng khỏi Slot.
        /// </summary>
        internal void Remove(int quantity)
        {
            _quantity -= quantity;

            if (_quantity <= 0)
            {
                Clear();
            }
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu trong Slot.
        /// </summary>
        internal void Clear()
        {
            _itemId = null;
            _quantity = 0;
        }
    }
}