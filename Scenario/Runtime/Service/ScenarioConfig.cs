using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Scenario.Service {
    
    [CreateAssetMenu(fileName = nameof(ScenarioConfig), menuName = "MisterGames/Events/" + nameof(ScenarioConfig))]
    public sealed class ScenarioConfig : ScriptableObject, IScenarioConfig {

        [SerializeField] private ScenarioEvent[] _scenarioEvents;

        public IReadOnlyList<ScenarioEvent> ScenarioEvents => _scenarioEvents ?? Array.Empty<ScenarioEvent>();
    }
    
}