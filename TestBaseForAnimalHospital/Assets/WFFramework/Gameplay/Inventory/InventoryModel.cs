using System;
using System.Collections.Generic;

namespace WFFramework.Gameplay.Inventory
{
    /// <summary>
    /// Logic runtime chính của Inventory.
    ///
    /// Đây là pure C# class:
    /// - Không phụ thuộc MonoBehaviour.
    /// - Không phụ thuộc UI.
    /// - Có thể Unit Test.
    /// - Có thể dùng cho Player, Bot, Chest hoặc Shop.
    /// </summary>
    public sealed class InventoryModel : IInventory
    {
        /// <summary>
        /// Catalog dùng để lấy MaxStack của Item.
        /// </summary>
        private readonly IItemCatalog _itemCatalog;

        /// <summary>
        /// Danh sách Slot runtime.
        /// </summary>
        private readonly List<InventorySlot> _slots;

        public string InventoryId { get; }

        public int Capacity => _slots.Count;

        public IReadOnlyList<InventorySlot> Slots => _slots;

        public event Action<InventoryChangedEvent> Changed;

        /// <summary>
        /// Tạo một Inventory mới.
        /// </summary>
        /// <param name="inventoryId">
        /// ID duy nhất của Inventory.
        /// </param>
        /// <param name="capacity">
        /// Số lượng Slot.
        /// </param>
        /// <param name="itemCatalog">
        /// Catalog cung cấp Item Definition.
        /// </param>
        public InventoryModel(
            string inventoryId,
            int capacity,
            IItemCatalog itemCatalog)
        {
            if (string.IsNullOrWhiteSpace(inventoryId))
            {
                throw new ArgumentException(
                    "InventoryId không được để trống.",
                    nameof(inventoryId));
            }

            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(capacity),
                    "Capacity phải lớn hơn 0.");
            }

            _itemCatalog = itemCatalog ??
                           throw new ArgumentNullException(
                               nameof(itemCatalog));

            InventoryId = inventoryId;

            _slots = new List<InventorySlot>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                _slots.Add(new InventorySlot());
            }
        }

        /// <summary>
        /// Lấy tổng quantity của Item trong toàn bộ Slot.
        /// </summary>
        public int GetQuantity(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            int totalQuantity = 0;

            for (int i = 0; i < _slots.Count; i++)
            {
                InventorySlot slot = _slots[i];

                if (slot.ItemId == itemId)
                {
                    totalQuantity += slot.Quantity;
                }
            }

            return totalQuantity;
        }

        /// <summary>
        /// Kiểm tra tổng không gian có thể chứa thêm Item.
        /// </summary>
        public bool CanAdd(
            string itemId,
            int quantity)
        {
            if (quantity <= 0)
            {
                return false;
            }

            IItemDefinition item =
                _itemCatalog.Get(itemId);

            int availableSpace = 0;

            for (int i = 0; i < _slots.Count; i++)
            {
                InventorySlot slot = _slots[i];

                if (slot.IsEmpty)
                {
                    availableSpace += item.MaxStack;
                }
                else if (slot.ItemId == itemId)
                {
                    availableSpace +=
                        item.MaxStack - slot.Quantity;
                }

                if (availableSpace >= quantity)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Thêm Item vào Inventory.
        ///
        /// Ưu tiên:
        /// 1. Stack đang tồn tại.
        /// 2. Slot trống.
        /// </summary>
        public bool TryAdd(
            string itemId,
            int quantity)
        {
            if (!CanAdd(itemId, quantity))
            {
                return false;
            }

            IItemDefinition item =
                _itemCatalog.Get(itemId);

            int remainingQuantity = quantity;

            // Giai đoạn 1:
            // Thêm vào các Stack đang tồn tại.
            for (int i = 0; i < _slots.Count; i++)
            {
                InventorySlot slot = _slots[i];

                if (slot.IsEmpty || slot.ItemId != itemId)
                {
                    continue;
                }

                int availableSpace =
                    item.MaxStack - slot.Quantity;

                int addedQuantity =
                    Math.Min(
                        availableSpace,
                        remainingQuantity);

                slot.Add(addedQuantity);
                remainingQuantity -= addedQuantity;

                if (remainingQuantity <= 0)
                {
                    break;
                }
            }

            // Giai đoạn 2:
            // Dùng các Slot trống nếu Stack cũ chưa đủ.
            if (remainingQuantity > 0)
            {
                for (int i = 0; i < _slots.Count; i++)
                {
                    InventorySlot slot = _slots[i];

                    if (!slot.IsEmpty)
                    {
                        continue;
                    }

                    int addedQuantity =
                        Math.Min(
                            item.MaxStack,
                            remainingQuantity);

                    slot.Set(itemId, addedQuantity);
                    remainingQuantity -= addedQuantity;

                    if (remainingQuantity <= 0)
                    {
                        break;
                    }
                }
            }

            Changed?.Invoke(
                new InventoryChangedEvent(
                    InventoryId,
                    itemId,
                    quantity));

            return true;
        }

        /// <summary>
        /// Xóa Item khỏi Inventory.
        ///
        /// Hàm kiểm tra đủ quantity trước khi thay đổi,
        /// tránh xóa một phần rồi thất bại.
        /// </summary>
        public bool TryRemove(
            string itemId,
            int quantity)
        {
            if (quantity <= 0)
            {
                return false;
            }

            if (GetQuantity(itemId) < quantity)
            {
                return false;
            }

            int remainingQuantity = quantity;

            // Xóa từ Slot cuối về đầu để dồn các Stack cũ phía trước.
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                InventorySlot slot = _slots[i];

                if (slot.ItemId != itemId)
                {
                    continue;
                }

                int removedQuantity =
                    Math.Min(
                        slot.Quantity,
                        remainingQuantity);

                slot.Remove(removedQuantity);
                remainingQuantity -= removedQuantity;

                if (remainingQuantity <= 0)
                {
                    break;
                }
            }

            Changed?.Invoke(
                new InventoryChangedEvent(
                    InventoryId,
                    itemId,
                    -quantity));

            return true;
        }
    }
}