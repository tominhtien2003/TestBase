using System.Collections.Generic;
using UnityEngine;

namespace WFFramework.Gameplay.Inventory
{
    /// <summary>
    /// ScriptableObject lưu danh sách toàn bộ Item của một game.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ItemCatalog",
        menuName = "WF/Gameplay/Item Catalog")]
    public sealed class ItemCatalog :
        ScriptableObject,
        IItemCatalog
    {
        /// <summary>
        /// Danh sách Item được thiết lập trong Inspector.
        /// </summary>
        [SerializeField]
        private List<ItemDefinitionBase> items =
            new List<ItemDefinitionBase>();

        /// <summary>
        /// Cache tìm Item theo ID với độ phức tạp O(1).
        /// </summary>
        private Dictionary<string, IItemDefinition> _itemMap;

        /// <summary>
        /// Thử tìm Item Definition theo ID.
        /// </summary>
        public bool TryGet(
            string itemId,
            out IItemDefinition item)
        {
            EnsureCache();

            return _itemMap.TryGetValue(itemId, out item);
        }

        /// <summary>
        /// Lấy Item Definition bắt buộc.
        /// </summary>
        public IItemDefinition Get(string itemId)
        {
            if (TryGet(itemId, out IItemDefinition item))
            {
                return item;
            }

            throw new KeyNotFoundException(
                $"Không tìm thấy ItemId: {itemId}");
        }

        /// <summary>
        /// Tạo Dictionary cache nếu chưa có.
        /// </summary>
        private void EnsureCache()
        {
            if (_itemMap != null)
            {
                return;
            }

            _itemMap =
                new Dictionary<string, IItemDefinition>();

            for (int i = 0; i < items.Count; i++)
            {
                ItemDefinitionBase item = items[i];

                if (item == null)
                {
                    Debug.LogError(
                        $"ItemCatalog có phần tử null tại index {i}.",
                        this);

                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.ItemId))
                {
                    Debug.LogError(
                        $"{item.name} chưa có ItemId.",
                        item);

                    continue;
                }

                if (_itemMap.ContainsKey(item.ItemId))
                {
                    Debug.LogError(
                        $"ItemId bị trùng: {item.ItemId}.",
                        item);

                    continue;
                }

                _itemMap.Add(item.ItemId, item);
            }
        }

        /// <summary>
        /// Xóa cache khi dữ liệu ScriptableObject bị thay đổi.
        /// </summary>
        private void OnValidate()
        {
            _itemMap = null;
        }
    }
}