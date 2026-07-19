using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFFramework.Gameplay.Stats
{
   /// <summary>
    /// Quy tắc khi áp dụng lại cùng một Buff.
    /// </summary>
    public enum BuffStackMode
    {
        /// <summary>
        /// Không cho áp dụng nếu Buff đã tồn tại.
        /// </summary>
        Ignore,

        /// <summary>
        /// Reset lại thời gian tồn tại của Buff hiện tại.
        /// </summary>
        RefreshDuration,

        /// <summary>
        /// Cho phép nhiều Instance cùng tồn tại.
        /// </summary>
        Stack
    }

    /// <summary>
    /// Dữ liệu Modifier được cấu hình trong Buff Definition.
    /// </summary>
    [Serializable]
    public sealed class BuffStatModifierData
    {
        /// <summary>
        /// Stat sẽ bị ảnh hưởng.
        /// </summary>
        [SerializeField]
        private StatDefinition stat;

        /// <summary>
        /// Kiểu thay đổi Stat.
        /// </summary>
        [SerializeField]
        private StatModifierOperation operation;

        /// <summary>
        /// Giá trị Modifier.
        /// </summary>
        [SerializeField]
        private float value;

        /// <summary>
        /// Priority khi Operation là Override.
        /// </summary>
        [SerializeField]
        private int priority;

        public StatDefinition Stat => stat;
        public StatModifierOperation Operation => operation;
        public float Value => value;
        public int Priority => priority;
    }

    /// <summary>
    /// ScriptableObject cấu hình một Buff.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Buff",
        menuName = "WF/Gameplay/Buff Definition")]
    public sealed class BuffDefinition : ScriptableObject
    {
        /// <summary>
        /// ID cố định của Buff.
        /// </summary>
        [SerializeField]
        private string buffId;

        /// <summary>
        /// Thời gian tồn tại tính bằng giây.
        ///
        /// Giá trị nhỏ hơn hoặc bằng 0 nghĩa là tồn tại vĩnh viễn
        /// cho đến khi bị Remove thủ công.
        /// </summary>
        [SerializeField, Min(0f)]
        private float duration;

        /// <summary>
        /// Quy tắc khi Buff được áp dụng lại.
        /// </summary>
        [SerializeField]
        private BuffStackMode stackMode;

        /// <summary>
        /// Các Stat Modifier mà Buff tạo ra.
        /// </summary>
        [SerializeField]
        private List<BuffStatModifierData> modifiers =
            new List<BuffStatModifierData>();

        public string BuffId => buffId;
        public float Duration => duration;
        public BuffStackMode StackMode => stackMode;

        public IReadOnlyList<BuffStatModifierData>
            Modifiers => modifiers;
    }
}