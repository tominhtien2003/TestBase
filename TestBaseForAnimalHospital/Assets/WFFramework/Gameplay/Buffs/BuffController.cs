using System;
using System.Collections.Generic;
using WFFramework.Core.Tick;

namespace WFFramework.Gameplay.Stats
{
    /// <summary>
    /// Quản lý Buff runtime của một Entity.
    ///
    /// Mỗi Player, Bot hoặc Patient có thể sở hữu một BuffController riêng.
    /// </summary>
    public sealed class BuffController :
        ITickable,
        IDisposable
    {
        /// <summary>
        /// StatCollection sẽ nhận Modifier từ Buff.
        /// </summary>
        private readonly StatCollection _stats;

        /// <summary>
        /// Các Buff Instance đang hoạt động.
        /// </summary>
        private readonly List<RuntimeBuff> _activeBuffs =
            new List<RuntimeBuff>();

        /// <summary>
        /// Handle đăng ký với TickScheduler.
        /// </summary>
        private readonly IDisposable _tickRegistration;

        public BuffController(
            StatCollection stats,
            ITickSystem tickSystem)
        {
            _stats = stats ??
                     throw new ArgumentNullException(
                         nameof(stats));

            if (tickSystem == null)
            {
                throw new ArgumentNullException(
                    nameof(tickSystem));
            }

            // Buff không cần Update mỗi frame.
            // Normal Tick thường đủ cho duration gameplay.
            _tickRegistration =
                tickSystem.Register(
                    TickGroup.Normal,
                    this);
        }

        /// <summary>
        /// Áp dụng một Buff.
        /// </summary>
        public bool Apply(BuffDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            RuntimeBuff existingBuff =
                FindFirstBuff(definition.BuffId);

            if (existingBuff != null)
            {
                switch (definition.StackMode)
                {
                    case BuffStackMode.Ignore:
                        return false;

                    case BuffStackMode.RefreshDuration:
                        existingBuff.RefreshDuration();
                        return true;

                    case BuffStackMode.Stack:
                        break;
                }
            }

            RuntimeBuff runtimeBuff =
                new RuntimeBuff(definition);

            ApplyModifiers(runtimeBuff);
            _activeBuffs.Add(runtimeBuff);

            return true;
        }

        /// <summary>
        /// Xóa tất cả Instance của một Buff.
        /// </summary>
        public int Remove(string buffId)
        {
            int removedCount = 0;

            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                RuntimeBuff buff = _activeBuffs[i];

                if (buff.Definition.BuffId != buffId)
                {
                    continue;
                }

                RemoveModifiers(buff);
                _activeBuffs.RemoveAt(i);

                removedCount++;
            }

            return removedCount;
        }

        /// <summary>
        /// Cập nhật duration của các Buff tạm thời.
        /// </summary>
        public void Tick(float deltaTime)
        {
            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                RuntimeBuff buff = _activeBuffs[i];

                // Duration <= 0 là Buff vĩnh viễn.
                if (buff.Definition.Duration <= 0f)
                {
                    continue;
                }

                buff.RemainingDuration -= deltaTime;

                if (buff.RemainingDuration > 0f)
                {
                    continue;
                }

                RemoveModifiers(buff);
                _activeBuffs.RemoveAt(i);
            }
        }

        /// <summary>
        /// Thêm toàn bộ Modifier của Buff vào StatCollection.
        /// </summary>
        private void ApplyModifiers(RuntimeBuff runtimeBuff)
        {
            IReadOnlyList<BuffStatModifierData> modifierData =
                runtimeBuff.Definition.Modifiers;

            for (int i = 0; i < modifierData.Count; i++)
            {
                BuffStatModifierData data =
                    modifierData[i];

                RuntimeStat stat =
                    _stats.Get(data.Stat);

                string modifierId =
                    $"{runtimeBuff.InstanceId}_{i}";

                StatModifier modifier =
                    new StatModifier(
                        modifierId,
                        runtimeBuff.InstanceId,
                        data.Operation,
                        data.Value,
                        data.Priority);

                stat.AddModifier(modifier);

                runtimeBuff.AppliedStats.Add(stat);
            }
        }

        /// <summary>
        /// Xóa Modifier mà Buff đã tạo.
        /// </summary>
        private static void RemoveModifiers(
            RuntimeBuff runtimeBuff)
        {
            for (int i = 0;
                 i < runtimeBuff.AppliedStats.Count;
                 i++)
            {
                runtimeBuff.AppliedStats[i]
                    .RemoveModifiersFromSource(
                        runtimeBuff.InstanceId);
            }

            runtimeBuff.AppliedStats.Clear();
        }

        /// <summary>
        /// Tìm Buff đầu tiên theo BuffId.
        /// </summary>
        private RuntimeBuff FindFirstBuff(string buffId)
        {
            for (int i = 0; i < _activeBuffs.Count; i++)
            {
                if (_activeBuffs[i]
                        .Definition.BuffId == buffId)
                {
                    return _activeBuffs[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Dọn toàn bộ Buff và Tick Registration.
        /// </summary>
        public void Dispose()
        {
            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                RemoveModifiers(_activeBuffs[i]);
            }

            _activeBuffs.Clear();
            _tickRegistration?.Dispose();
        }

        /// <summary>
        /// Một Instance runtime của Buff.
        /// </summary>
        private sealed class RuntimeBuff
        {
            /// <summary>
            /// ID riêng của Instance.
            ///
            /// Hai Buff stack cùng loại vẫn có InstanceId khác nhau.
            /// </summary>
            public readonly string InstanceId;

            public readonly BuffDefinition Definition;

            /// <summary>
            /// Các RuntimeStat đã nhận Modifier.
            ///
            /// Dùng để xóa Modifier khi Buff kết thúc.
            /// </summary>
            public readonly List<RuntimeStat> AppliedStats =
                new List<RuntimeStat>();

            public float RemainingDuration;

            public RuntimeBuff(BuffDefinition definition)
            {
                Definition = definition;

                InstanceId =
                    Guid.NewGuid().ToString("N");

                RemainingDuration =
                    definition.Duration;
            }

            /// <summary>
            /// Reset lại duration theo Definition.
            /// </summary>
            public void RefreshDuration()
            {
                RemainingDuration =
                    Definition.Duration;
            }
        }
    }
}