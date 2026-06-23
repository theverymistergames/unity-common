using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Scenario.Service {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class ScenarioServiceRunner : MonoBehaviour {
        
        private readonly ScenarioService _scenarioService = new();
        
        private void Awake() {
            _scenarioService.Initialize();
            Services.Register<IScenarioService>(_scenarioService);
        }

        private void OnDestroy() {
            Services.Unregister(_scenarioService);
            _scenarioService.Dispose();
        }
    }
    
}