using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Scenario.Service {
    
    public sealed class ScenarioLauncher : MonoBehaviour {
        
        [SerializeField] private ScenarioConfig _scenarioConfig;
        
        private void Awake() {
            Services.Get<IScenarioService>()?.AddScenario(_scenarioConfig);
        }

        private void OnDestroy() {
            Services.Get<IScenarioService>()?.RemoveScenario(_scenarioConfig);
        }
    }
    
}