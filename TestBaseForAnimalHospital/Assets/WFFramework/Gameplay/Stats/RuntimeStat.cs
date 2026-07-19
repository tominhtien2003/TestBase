using System;
using System.Collections.Generic;

namespace WFFramework.Gameplay.Stats
{
   /// <summary>
    /// Cách một Modifier ảnh hưởng đến Stat.
    /// </summary>
    public enum StatModifierOperation
    {
        /// <summary>
        /// Cộng trực tiếp vào Base Value.
        ///
        /// Ví dụ Base 3 + Add 1 = 4.
        /// </summary>
        Add,

        /// <summary>
        /// Nhân với giá trị hiện tại.
        ///
        /// Ví dụ Value 4 × Multiply 1.5 = 6.
        /// </summary>
        Multiply,

        /// <summary>
        /// Ghi đè kết quả cuối cùng.
        ///
        /// Override có Priority cao nhất sẽ được sử dụng.
        /// </summary>
        Override
    }

    /// <summary>
    /// Một Modifier runtime của Stat.
    /// </summary>
    public readonly struct StatModifier
    {
        /// <summary>
        /// ID duy nhất của Modifier.
        /// </summary>
        public readonly string ModifierId;

        /// <summary>
        /// ID của hệ thống tạo Modifier.
        ///
        /// Ví dụ:
        /// Buff ID, Equipment ID hoặc Debuff ID.
        /// </summary>
        public readonly string SourceId;

        public readonly StatModifierOperation Operation;
        public readonly float Value;

        /// <summary>
        /// Độ ưu tiên, chủ yếu dùng cho Override.
        /// </summary>
        public readonly int Priority;

        public StatModifier(
            string modifierId,
            string sourceId,
            StatModifierOperation operation,
            float value,
            int priority = 0)
        {
            ModifierId = modifierId;
            SourceId = sourceId;
            Operation = operation;
            Value = value;
            Priority = priority;
        }
    }

    /// <summary>
    /// Một Stat runtime có Base Value và danh sách Modifier.
    /// </summary>
    public sealed class RuntimeStat
    {
        /// <summary>
        /// Giá trị gốc trước Modifier.
        /// </summary>
        private float _baseValue;

        /// <summary>
        /// Danh sách Modifier đang hoạt động.
        /// </summary>
        private readonly List<StatModifier> _modifiers =
            new List<StatModifier>();

        public float BaseValue => _baseValue;

        /// <summary>
        /// Giá trị cuối cùng sau khi tính toàn bộ Modifier.
        /// </summary>
        public float Value => CalculateValue();

        /// <summary>
        /// Event gọi khi Base Value hoặc Modifier thay đổi.
        /// </summary>
        public event Action Changed;

        public RuntimeStat(float baseValue)
        {
            _baseValue = baseValue;
        }

        /// <summary>
        /// Thay đổi Base Value.
        /// </summary>
        public void SetBaseValue(float value)
        {
            if (Math.Abs(_baseValue - value) < 0.0001f)
            {
                return;
            }

            _baseValue = value;
            Changed?.Invoke();
        }

        /// <summary>
        /// Thêm một Modifier.
        ///
        /// ModifierId không được trùng.
        /// </summary>
        public bool AddModifier(StatModifier modifier)
        {
            for (int i = 0; i < _modifiers.Count; i++)
            {
                if (_modifiers[i].ModifierId ==
                    modifier.ModifierId)
                {
                    return false;
                }
            }

            _modifiers.Add(modifier);
            Changed?.Invoke();

            return true;
        }

        /// <summary>
        /// Xóa Modifier theo ID.
        /// </summary>
        public bool RemoveModifier(string modifierId)
        {
            for (int i = 0; i < _modifiers.Count; i++)
            {
                if (_modifiers[i].ModifierId != modifierId)
                {
                    continue;
                }

                _modifiers.RemoveAt(i);
                Changed?.Invoke();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Xóa toàn bộ Modifier được tạo bởi một Source.
        ///
        /// Ví dụ tháo một Equipment hoặc xóa một Buff.
        /// </summary>
        public int RemoveModifiersFromSource(
            string sourceId)
        {
            int removedCount = 0;

            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].SourceId != sourceId)
                {
                    continue;
                }

                _modifiers.RemoveAt(i);
                removedCount++;
            }

            if (removedCount > 0)
            {
                Changed?.Invoke();
            }

            return removedCount;
        }

        /// <summary>
        /// Tính giá trị cuối cùng.
        ///
        /// Công thức:
        /// (Base + tổng Add) × toàn bộ Multiply
        ///
        /// Nếu có Override, Override Priority cao nhất được dùng cuối cùng.
        /// </summary>
        private float CalculateValue()
        {
            float result = _baseValue;

            // Bước 1: cộng toàn bộ Add Modifier.
            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier modifier = _modifiers[i];

                if (modifier.Operation ==
                    StatModifierOperation.Add)
                {
                    result += modifier.Value;
                }
            }

            // Bước 2: nhân toàn bộ Multiply Modifier.
            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier modifier = _modifiers[i];

                if (modifier.Operation ==
                    StatModifierOperation.Multiply)
                {
                    result *= modifier.Value;
                }
            }

            // Bước 3: tìm Override có Priority cao nhất.
            bool hasOverride = false;
            int highestPriority = int.MinValue;
            float overrideValue = result;

            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier modifier = _modifiers[i];

                if (modifier.Operation !=
                    StatModifierOperation.Override)
                {
                    continue;
                }

                if (!hasOverride ||
                    modifier.Priority > highestPriority)
                {
                    hasOverride = true;
                    highestPriority = modifier.Priority;
                    overrideValue = modifier.Value;
                }
            }

            return hasOverride
                ? overrideValue
                : result;
        }
    }
}