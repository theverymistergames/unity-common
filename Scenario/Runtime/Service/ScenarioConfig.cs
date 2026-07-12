using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Scenario.Service {
    
    [CreateAssetMenu(fileName = nameof(ScenarioConfig), menuName = "MisterGames/Events/" + nameof(ScenarioConfig))]
    public sealed class ScenarioConfig : ScriptableObject, IScenarioConfig {

        [HideInInspector]
        [SerializeField] private int _id;
        [SerializeField] private Group[] _groups;

        [Serializable]
        private struct Group {
            public string name;
            [TextAreaExtended]
            public string description;
            public ScenarioEvent[] scenarioEvents;
        }

        public void CollectAllScenarioEvents(List<ScenarioEvent> buffer) {
            for (int i = 0; i < _groups.Length; i++) {
                buffer.AddRange(_groups[i].scenarioEvents);
            }
        }

#if UNITY_EDITOR
        private readonly HashSet<int> _occupiedIdsBuffer = new();
        
        private void OnValidate() {
            _occupiedIdsBuffer.Clear();
            
            for (int i = 0; i < _groups.Length; i++) {
                ref var group = ref _groups[i];
                for (int j = 0; j < group.scenarioEvents.Length; j++) {
                    var evt = group.scenarioEvents[j];

                    if (evt.id != 0 && _occupiedIdsBuffer.Add(evt.id)) {
                        continue;
                    }
                    
                    evt.id = GetNextId();
                    _occupiedIdsBuffer.Add(evt.id);
                    
                    evt.name = null;
                    evt.description = null;
                    evt.startEvent = default;
                    evt.subIdMode = ScenarioEvent.SubIdMode.IgnoreSubId;
                    evt.expectedRaiseCount = default;
                    evt.condition = null;
                    evt.actionMode = ScenarioEvent.ActionMode.FireAndForget;
                    evt.startAction = null;
                }
            }
        }

        private int GetNextId() {
            _id.IncrementUncheckedRef();
            if (_id == 0) _id.IncrementUncheckedRef();
            
            return _id;
        }
#endif
    }
    
}