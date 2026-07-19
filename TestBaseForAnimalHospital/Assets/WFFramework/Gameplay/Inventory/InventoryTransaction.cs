namespace WFFramework.Gameplay.Inventory
{
    /// <summary>
    /// Utility xử lý giao dịch giữa hai Inventory.
    ///
    /// Mục tiêu:
    /// - Không làm mất Item.
    /// - Không Remove khi Inventory đích không đủ chỗ.
    /// - Rollback nếu xảy ra lỗi bất thường.
    /// </summary>
    public static class InventoryTransaction
    {
        /// <summary>
        /// Chuyển Item từ Inventory nguồn sang Inventory đích.
        /// </summary>
        public static bool TryTransfer(
            IInventory source,
            IInventory destination,
            string itemId,
            int quantity)
        {
            if (source == null ||
                destination == null ||
                string.IsNullOrWhiteSpace(itemId) ||
                quantity <= 0)
            {
                return false;
            }

            // Inventory nguồn không đủ Item.
            if (source.GetQuantity(itemId) < quantity)
            {
                return false;
            }

            // Inventory đích không đủ chỗ.
            if (!destination.CanAdd(itemId, quantity))
            {
                return false;
            }

            // Chỉ Remove sau khi đã kiểm tra destination.
            if (!source.TryRemove(itemId, quantity))
            {
                return false;
            }

            if (destination.TryAdd(itemId, quantity))
            {
                return true;
            }

            // Rollback trong trường hợp Add thất bại bất thường.
            source.TryAdd(itemId, quantity);

            return false;
        }
    }
}