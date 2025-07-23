using System;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Common.Stats {
    
    [Serializable]
    public sealed class StatModifier : IStatModifier {
        
        [SerializeField] private LabelValue _statType;
        [SerializeField] private ModifierPriority _priority;
        [SerializeField] private OperationType _operation;
        [SerializeField] private float _modifier;
        [SerializeField] private float _duration;
        [SerializeField] private ModifierOptions _options;
        [SerializeField] private LabelValue _group;

        public int Priority => (int) _priority;
        public int Order => (int) _operation;
        public float Duration => _duration;
        public int StatType => _statType.GetValue();
        public ModifierOptions Options => _options;
        public int Group => _group.GetValue();
        
        // Do not remove: Odin Inspector does not show a class in the [SerializeReference] dropdown menu,
        // if there is no empty constructor.
        public StatModifier() { }
        
        public StatModifier(
            LabelValue statType,
            ModifierPriority priority,
            OperationType operation,
            float modifier,
            float duration = 0f,
            ModifierOptions options = ModifierOptions.None)
        {
            _statType = statType;
            _priority = priority;
            _operation = operation;
            _modifier = modifier;
            _duration = duration;
            _options = options;
        }

        public float Modify(float value) {
            return _operation.Apply(value, _modifier);
        }

        public override string ToString() {
            return $"{nameof(StatModifier)}({_statType} {_priority} {_operation}: {_modifier}, group {_group}, duration {_duration}, options {_options})";
        }
    }
    
}