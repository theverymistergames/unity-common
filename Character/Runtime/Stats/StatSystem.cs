using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.Pool;

namespace MisterGames.Character.Stats {
    
    public sealed class StatSystem : IStatSystem, IUpdate, IComparer<StatSystem.ModifierData> {
        
        private const bool PrintLogs = true;

        public event Action OnUpdateModifiers = delegate { };

        private readonly List<ModifierData> _modifiersData = new();
        private readonly Dictionary<int, IStatModifier> _modifiersMap = new();
        private readonly Dictionary<int, ConditionData> _conditionsMap = new();
        private readonly Dictionary<GroupKey, int> _groupToOverwriteIdMap = new();
        private readonly Dictionary<GroupKey, int> _groupToRemoveLastIdMap = new();

        private readonly struct ModifierData {
            public readonly int id;
            public readonly int source;
            public readonly float startTime;
            public readonly float duration;

            public ModifierData(int id, int source, float startTime, float duration) {
                this.id = id;
                this.source = source;
                this.startTime = startTime;
                this.duration = duration;
            }
        }

        private readonly struct ConditionData {
            public readonly IActorCondition condition;
            public readonly float startTime;
            public readonly bool result;

            public ConditionData(IActorCondition condition, float startTime, bool result) {
                this.condition = condition;
                this.result = result;
                this.startTime = startTime;
            }
        }

        private readonly struct GroupKey : IEquatable<GroupKey> {
            private readonly int _statType;
            private readonly int _priority;
            private readonly int _group;

            public GroupKey(int statType, int priority, int group) {
                _statType = statType;
                _priority = priority;
                _group = group;
            }

            public bool Equals(GroupKey other) {
                return _statType == other._statType && _priority == other._priority && _group == other._group;
            }

            public override bool Equals(object obj) => obj is GroupKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(_statType, _priority, _group);
            public static bool operator ==(GroupKey left, GroupKey right) => left.Equals(right);
            public static bool operator !=(GroupKey left, GroupKey right) => !left.Equals(right);
        }

        private IActor _actor;
        private int _lastId;
        private bool _notifyFlag;

        public void Bind(IActor actor) {
            _actor = actor;
        }

        public void Unbind() {
            _actor = null;
        }
        
        public int ModifyValue(int statType, int value) {
            return Mathf.CeilToInt(ModifyValue(statType, (float) value));
        }

        public float ModifyValue(int statType, float value) {
            // Modifiers are sorted by StatType, which allows not to iterate over all the entries.
            // Iteration skips until passed StatType is found, then continues until next StatType is reached.
            bool encounteredStatTypeEntry = false;

            for (int i = 0; i < _modifiersData.Count; i++) {
                var data = _modifiersData[i];

                if (!_modifiersMap.TryGetValue(data.id, out var modifier) || modifier.StatType != statType) {
                    if (encounteredStatTypeEntry) break;
                    continue;
                }

                encounteredStatTypeEntry = true;

                if ((_conditionsMap.TryGetValue(data.id, out var conditionData) && !conditionData.result) ||
                    ((modifier.Options & ModifierOptions.OverwriteLast) == ModifierOptions.OverwriteLast &&
                     _groupToOverwriteIdMap.TryGetValue(new GroupKey(statType, modifier.Priority, modifier.Group), out int overwriteId) && data.id != overwriteId)) 
                {
                    continue;
                }

                value = modifier.Modify(value);
            }

            return value;
        }

        public void ForceNotifyUpdate() {
#if UNITY_EDITOR
            if (PrintLogs) Debug.LogWarning($"{nameof(StatSystem)}: force update modifiers. " +
                                            $"State:\n{GetModifiersStateAsString()}");
#endif

            _notifyFlag = false;
            OnUpdateModifiers.Invoke();
        }

        public void AddModifier(object source, IStatModifier modifier, IActorCondition condition = null) {
            if (source == null || modifier == null) return;

            AddModifierEntry(source, modifier, condition);

            _modifiersData.Sort(this);

#if UNITY_EDITOR
            if (PrintLogs) Debug.LogWarning($"{nameof(StatSystem)}: added modifier {modifier} " +
                                            $"from source [{source}], " +
                                            $"condition [{condition}]. " +
                                            $"State:\n{GetModifiersStateAsString()}");
#endif

            _notifyFlag = true;
        }

        public void AddModifiers(object source, IReadOnlyList<IStatModifier> modifiers, IActorCondition condition = null) {
            if (source == null || modifiers is not { Count: > 0 }) return;

            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i] is { } modifier) AddModifierEntry(source, modifier, condition);
            }

            _modifiersData.Sort(this);

#if UNITY_EDITOR
            if (PrintLogs) Debug.LogWarning($"{nameof(StatSystem)}: added modifiers\n- {string.Join("\n- ", modifiers)}\n" +
                                            $"from source [{source}], " +
                                            $"condition [{condition}]. " +
                                            $"State:\n{GetModifiersStateAsString()}");
#endif

            _notifyFlag = true;
        }

        public void RemoveModifier(object source, IStatModifier modifier) {
            if (source == null || modifier == null) return;

            int hash = source.GetHashCode();
            bool changed = false;

            for (int i = _modifiersData.Count - 1; i >= 0; i--) {
                var data = _modifiersData[i];

                if (data.source != hash ||
                    !_modifiersMap.TryGetValue(data.id, out var mod) ||
                    modifier != mod) 
                {
                    continue;
                }

                _modifiersMap.Remove(data.id);
                _conditionsMap.Remove(data.id);
                _modifiersData.RemoveAt(i);
                _groupToOverwriteIdMap.Remove(CreateGroupKey(mod));
                changed = true;
            }

            if (!changed) return;

#if UNITY_EDITOR
            if (PrintLogs) Debug.LogWarning($"{nameof(StatSystem)}: removed modifier {modifier} " +
                                            $"from source [{source}]. " +
                                            $"State:\n{GetModifiersStateAsString()}");
#endif

            _notifyFlag = true;
        }

        public void RemoveModifiers(object source, IReadOnlyList<IStatModifier> modifiers) {
            if (source == null || modifiers is not { Count: > 0 }) return;

            int hash = source.GetHashCode();
            bool changed = false;

            for (int i = _modifiersData.Count - 1; i >= 0; i--) {
                var data = _modifiersData[i];

                if (data.source != hash ||
                    !_modifiersMap.TryGetValue(data.id, out var mod)) 
                {
                    continue;
                }

                for (int j = 0; j < modifiers.Count; j++) {
                    if (mod != modifiers[j]) continue;

                    _modifiersMap.Remove(data.id);
                    _conditionsMap.Remove(data.id);
                    _modifiersData.RemoveAt(i);
                    _groupToOverwriteIdMap.Remove(CreateGroupKey(mod));
                    changed = true;

                    break;
                }
            }

            if (!changed) return;

#if UNITY_EDITOR
            if (PrintLogs) Debug.LogWarning($"{nameof(StatSystem)}: removed modifiers\n- {string.Join("\n- ", modifiers)}\n" +
                                            $"from source [{source}]. " +
                                            $"State:\n{GetModifiersStateAsString()}");
#endif

            _notifyFlag = true;
        }

        public void RemoveModifiersOf(object source) {
            if (source == null) return;

            int hash = source.GetHashCode();
            bool changed = false;

            for (int i = _modifiersData.Count - 1; i >= 0; i--) {
                var data = _modifiersData[i];
                if (data.source != hash) continue;

                _modifiersMap.Remove(data.id, out var modifier);
                _conditionsMap.Remove(data.id);
                _modifiersData.RemoveAt(i);
                _groupToOverwriteIdMap.Remove(CreateGroupKey(modifier));

                changed = true;
            }

            if (!changed) return;

#if UNITY_EDITOR
            if (PrintLogs) Debug.LogWarning($"{nameof(StatSystem)}: removed all modifiers " +
                                            $"from source [{source}]. " +
                                            $"State:\n{GetModifiersStateAsString()}");
#endif

            _notifyFlag = true;
        }

        public void Clear() {
            _modifiersData.Clear();
            _modifiersMap.Clear();
            _conditionsMap.Clear();
            _groupToOverwriteIdMap.Clear();
            _groupToRemoveLastIdMap.Clear();

#if UNITY_EDITOR
            if (PrintLogs) Debug.LogWarning($"{nameof(StatSystem)}: cleared all modifiers from all sources. " +
                                            $"State:\n{GetModifiersStateAsString()}");
#endif

            OnUpdateModifiers.Invoke();
        }

        void IUpdate.OnUpdate(float dt) {
            _groupToRemoveLastIdMap.Clear();

            bool changed = false;
            float time = Time.time;

#if UNITY_EDITOR
            var removedModifiers = ListPool<IStatModifier>.Get();
#endif

            for (int i = _modifiersData.Count - 1; i >= 0; i--) {
                var data = _modifiersData[i];

                // Do not notify changes: modifier was already deleted but not cleaned up.
                if (!_modifiersMap.TryGetValue(data.id, out var modifier)) {
                    _conditionsMap.Remove(data.id);
                    _modifiersData.RemoveAt(i);
                    continue;
                }

                // Remove if has duration and it is expired.
                if (data.duration > 0f && time >= data.startTime + data.duration) {
                    _modifiersMap.Remove(data.id);
                    _conditionsMap.Remove(data.id);
                    _modifiersData.RemoveAt(i);
                    changed = true;

#if UNITY_EDITOR
                    removedModifiers.Add(modifier);
#endif
                    continue;
                }

                // No condition means positive result.
                bool conditionResult = true;

                if (_conditionsMap.TryGetValue(data.id, out var conditionData)) {
                    conditionResult = conditionData.condition.IsMatch(_actor, conditionData.startTime);

                    if (conditionData.result != conditionResult) {
                        conditionData = new ConditionData(conditionData.condition, conditionData.startTime, conditionResult);
                        _conditionsMap[data.id] = conditionData;
                        changed = true;
                    }
                }

                // Remove modifier if condition is negative and modifier has option RemoveOnConditionFail.
                if (!conditionResult &&
                    (modifier.Options & ModifierOptions.RemoveOnConditionFail) == ModifierOptions.RemoveOnConditionFail) 
                {
                    _modifiersMap.Remove(data.id);
                    _conditionsMap.Remove(data.id);
                    _modifiersData.RemoveAt(i);
                    changed = true;

#if UNITY_EDITOR
                    removedModifiers.Add(modifier);
#endif
                }

                var groupKey = CreateGroupKey(modifier);

                // Set group overwrite id:
                // it is the last added modifier with positive condition result and option OverwriteLast.
                if (conditionResult &&
                    (modifier.Options & ModifierOptions.OverwriteLast) == ModifierOptions.OverwriteLast &&
                    (!_groupToOverwriteIdMap.TryGetValue(groupKey, out int overwriteId) || data.id > overwriteId)) 
                {
                    _groupToOverwriteIdMap[groupKey] = data.id;
                    changed = true;
                }

                // Track last modifier in group with negative condition result and option RemoveLastOnConditionFail:
                // to remove last modifiers in groups later. 
                if (!conditionResult &&
                    (modifier.Options & ModifierOptions.RemoveLastOnConditionFail) == ModifierOptions.RemoveLastOnConditionFail) 
                {
                    if (!_groupToRemoveLastIdMap.TryGetValue(groupKey, out int removeId) || data.id > removeId) {
                        _groupToRemoveLastIdMap[groupKey] = data.id;
                        changed = true;
                    }

                    // Update condition start time to current and set positive result,
                    // to only apply negative condition result to the last modifier in group.
                    _conditionsMap[data.id] = new ConditionData(conditionData.condition, time, true);
                }
            }

            foreach (int id in _groupToRemoveLastIdMap.Values) {
#if UNITY_EDITOR
                if (_modifiersMap.TryGetValue(id, out var modifier)) removedModifiers.Add(modifier);
#endif

                _modifiersMap.Remove(id);
                _conditionsMap.Remove(id);
            }

#if UNITY_EDITOR
            if (removedModifiers.Count > 0 && PrintLogs) {
                Debug.LogWarning($"{nameof(StatSystem)}: removed modifiers\n- {string.Join("\n- ", removedModifiers)}\n" +
                                                                          $"State:\n{GetModifiersStateAsString()}");
            }
            ListPool<IStatModifier>.Release(removedModifiers);
#endif

            if (!changed && !_notifyFlag) return;

            _notifyFlag = false;
            OnUpdateModifiers.Invoke();
        }

        private void AddModifierEntry(object source, IStatModifier modifier, IActorCondition condition) {
            int id = _lastId++;
            int sourceHash = source.GetHashCode();
            float startTime = Time.time;

            _modifiersMap[id] = modifier;
            _modifiersData.Add(new ModifierData(id, sourceHash, startTime, modifier.Duration));

            if (condition != null) _conditionsMap[id] = new ConditionData(condition, startTime, condition.IsMatch(_actor, startTime));
        }

        private static GroupKey CreateGroupKey(IStatModifier modifier) {
            return new GroupKey(modifier.StatType, modifier.Priority, modifier.Group);
        }

        // Order by:
        // 1. Stat type
        // 2. Priority asc
        // 3. Order asc
        // 4. Id
        int IComparer<ModifierData>.Compare(ModifierData x, ModifierData y) {
            if (!_modifiersMap.TryGetValue(x.id, out var modX) ||
                !_modifiersMap.TryGetValue(y.id, out var modY)) 
            {
                return 0;
            }

            int statType = modX.StatType - modY.StatType;
            if (statType != 0) return statType;

            int priority = modX.Priority - modY.Priority;
            if (priority != 0) return priority;

            int order = modX.Order - modY.Order;
            if (order != 0) return order;

            return x.id - y.id;
        }

#if UNITY_EDITOR
        private string GetModifiersStateAsString() {
            var sb = new StringBuilder();

            sb.AppendLine("Modifiers: ");

            for (int i = 0; i < _modifiersData.Count; i++) {
                var data = _modifiersData[i];
                if (!_modifiersMap.TryGetValue(data.id, out var modifier)) continue;

                sb.AppendLine($" - #{data.id} [{modifier}], condition [{(_conditionsMap.TryGetValue(data.id, out var c) ? c.condition : "")}]");
            }

            return sb.ToString();
        }
#endif
    }
}