using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Scenario.Service {
    
    [CreateAssetMenu(fileName = nameof(ScenarioConfig), menuName = "MisterGames/Events/" + nameof(ScenarioConfig))]
    public sealed class ScenarioConfig : ScriptableObject, IScenarioConfig {

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
    }
    
}