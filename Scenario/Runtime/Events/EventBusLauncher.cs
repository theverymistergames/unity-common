using UnityEngine;

namespace MisterGames.Scenario.Events {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class EventBusLauncher : MonoBehaviour {
        
        private readonly EventBus _eventBus = new();

        private void OnDestroy() {
            _eventBus.Dispose();
        }
    }

}
