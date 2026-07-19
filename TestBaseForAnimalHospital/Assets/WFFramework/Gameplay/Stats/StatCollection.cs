using System;
using System.Collections.Generic;

namespace WFFramework.Gameplay.Stats
{
    /// <summary>
    /// Tập hợp RuntimeStat của một Entity.
    ///
    /// Ví dụ Patient có:
    /// - MoveSpeed.
    /// - RecoverySpeed.
    /// - HealthDecay.
    /// </summary>
    public sealed class StatCollection
    {
        /// <summary>
        /// StatId → RuntimeStat.
        /// </summary>
        private readonly Dictionary<string, RuntimeStat> _stats =
            new Dictionary<string, RuntimeStat>();

        /// <summary>
        /// Thêm một Stat vào Collection.
        /// </summary>
        public RuntimeStat AddStat(
            StatDefinition definition,
            float? customBaseValue = null)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (_stats.ContainsKey(definition.StatId))
            {
                throw new InvalidOperationException(
                    $"Stat đã tồn tại: {definition.StatId}");
            }

            float baseValue =
                customBaseValue ??
                definition.DefaultBaseValue;

            RuntimeStat stat =
                new RuntimeStat(baseValue);

            _stats.Add(definition.StatId, stat);

            return stat;
        }

        /// <summary>
        /// Lấy RuntimeStat theo Definition.
        /// </summary>
        public RuntimeStat Get(
            StatDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            return Get(definition.StatId);
        }

        /// <summary>
        /// Lấy RuntimeStat theo ID.
        /// </summary>
        public RuntimeStat Get(string statId)
        {
            if (_stats.TryGetValue(
                    statId,
                    out RuntimeStat stat))
            {
                return stat;
            }

            throw new KeyNotFoundException(
                $"Không tìm thấy Stat: {statId}");
        }

        /// <summary>
        /// Thử lấy RuntimeStat mà không ném exception.
        /// </summary>
        public bool TryGet(
            string statId,
            out RuntimeStat stat)
        {
            return _stats.TryGetValue(statId, out stat);
        }
    }
}