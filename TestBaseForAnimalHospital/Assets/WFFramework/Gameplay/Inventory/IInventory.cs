using System;
using System.Collections.Generic;

namespace WFFramework.Gameplay.Inventory
{
    /// <summary>
    /// Dữ liệu mô tả một thay đổi trong Inventory.
    /// </summary>
    public readonly struct InventoryChangedEvent
    {
        /// <summary>
        /// ID Inventory đã thay đổi.
        /// </summary>
        public readonly string InventoryId;

        /// <summary>
        /// Item bị thay đổi.
        /// </summary>
        public readonly string ItemId;

        /// <summary>
        /// Số lượng thay đổi.
        ///
        /// Số dương là Add, số âm là Remove.
        /// </summary>
        public readonly int QuantityDelta;

        public InventoryChangedEvent(
            string inventoryId,
            string itemId,
            int quantityDelta)
        {
            InventoryId = inventoryId;
            ItemId = itemId;
            QuantityDelta = quantityDelta;
        }
    }

    /// <summary>
    /// Interface public của một Inventory runtime.
    /// </summary>
    public interface IInventory
    {
        /// <summary>
        /// ID duy nhất của Inventory.
        /// </summary>
        string InventoryId { get; }

        /// <summary>
        /// Tổng số Slot.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Danh sách Slot chỉ đọc từ bên ngoài.
        /// </summary>
        IReadOnlyList<InventorySlot> Slots { get; }

        /// <summary>
        /// Event được gọi khi Inventory thay đổi.
        /// </summary>
        event Action<InventoryChangedEvent> Changed;

        /// <summary>
        /// Lấy tổng quantity của một Item trong toàn bộ Inventory.
        /// </summary>
        int GetQuantity(string itemId);

        /// <summary>
        /// Kiểm tra Inventory có đủ chỗ để thêm Item hay không.
        /// </summary>
        bool CanAdd(
            string itemId,
            int quantity);

        /// <summary>
        /// Thêm Item theo cơ chế all-or-nothing.
        ///
        /// Nếu không đủ chỗ, Inventory không thay đổi.
        /// </summary>
        bool TryAdd(
            string itemId,
            int quantity);

        /// <summary>
        /// Xóa Item theo cơ chế all-or-nothing.
        ///
        /// Nếu không đủ quantity, Inventory không thay đổi.
        /// </summary>
        bool TryRemove(
            string itemId,
            int quantity);
    }
}