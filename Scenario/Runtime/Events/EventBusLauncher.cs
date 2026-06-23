using UnityEngine;

namespace MisterGames.Scenario.Events {
    
    [DefaultExecutionOrder(-10000)]
    public sealed class EventBusLauncher : MonoBehaviour {
        
        private void OnDestroy() {
            EventBus.Main.Dispose();
        }
    }

}
