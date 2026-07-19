using UnityEngine;

namespace WFFramework.Gameplay.Inventory
{
    /// <summary>
    /// Interface tối thiểu mà Inventory cần từ một Item.
    /// </summary>
    public interface IItemDefinition
    {
        /// <summary>
        /// ID cố định của Item.
        ///
        /// ID này được dùng trong SaveData,
        /// không nên thay đổi sau khi game đã phát hành.
        /// </summary>
        string ItemId { get; }

        /// <summary>
        /// Số lượng tối đa trong một Inventory Slot.
        ///
        /// MaxStack = 1 nghĩa là Item không thể stack.
        /// </summary>
        int MaxStack { get; }
    }

    /// <summary>
    /// ScriptableObject base cho Item dùng chung giữa nhiều game.
    ///
    /// Game cụ thể có thể kế thừa class này để thêm dữ liệu riêng.
    /// </summary>
    public abstract class ItemDefinitionBase :
        ScriptableObject,
        IItemDefinition
    {
        /// <summary>
        /// ID cố định của Item.
        /// </summary>
        [SerializeField]
        private string itemId;

        /// <summary>
        /// Tên hiển thị trong UI.
        /// </summary>
        [SerializeField]
        private string displayName;

        /// <summary>
        /// Icon chung của Item.
        /// </summary>
        [SerializeField]
        private Sprite icon;

        /// <summary>
        /// Số Item tối đa trong một Slot.
        /// </summary>
        [SerializeField, Min(1)]
        private int maxStack = 99;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public int MaxStack => maxStack;
    }
}