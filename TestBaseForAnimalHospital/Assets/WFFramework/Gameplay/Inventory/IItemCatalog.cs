namespace WFFramework.Gameplay.Inventory
{
    /// <summary>
    /// Catalog dùng để tìm ItemDefinition từ ItemId.
    ///
    /// Khi Load SaveData, Inventory chỉ có ItemId,
    /// vì vậy cần Catalog để lấy lại Definition.
    /// </summary>
    public interface IItemCatalog
    {
        /// <summary>
        /// Thử tìm Item Definition theo ID.
        /// </summary>
        bool TryGet(
            string itemId,
            out IItemDefinition item);

        /// <summary>
        /// Lấy Item theo ID.
        ///
        /// Hàm ném exception nếu Item không tồn tại.
        /// </summary>
        IItemDefinition Get(string itemId);
    }
}