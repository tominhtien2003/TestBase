using UnityEngine;

namespace WFFramework.Gameplay.Stats
{
    /// <summary>
    /// ScriptableObject định nghĩa một loại Stat.
    ///
    /// Ví dụ:
    /// - MoveSpeed.
    /// - RecoverySpeed.
    /// - Damage.
    /// - HealthDecay.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Stat",
        menuName = "WF/Gameplay/Stat Definition")]
    public sealed class StatDefinition : ScriptableObject
    {
        /// <summary>
        /// ID cố định của Stat.
        /// </summary>
        [SerializeField]
        private string statId;

        /// <summary>
        /// Giá trị mặc định khi tạo RuntimeStat.
        /// </summary>
        [SerializeField]
        private float defaultBaseValue = 1f;

        public string StatId => statId;
        public float DefaultBaseValue => defaultBaseValue;
    }
}