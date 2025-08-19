using UnityEngine;

namespace MisterGames.Common.Labels {
    
    [DefaultExecutionOrder(-100_000)]
    internal sealed class LabelLibrariesRunner : MonoBehaviour {

        public static ILabelValueEventSystem EventSystem => _eventSystem;
        public static ILabelValueRuntimeStorage RuntimeStorage => _runtimeStorage;
        
        private static readonly LabelValueEventSystem _eventSystem = new();
        private static readonly LabelValueRuntimeStorage _runtimeStorage = new();

        private void OnDestroy() {
            _eventSystem.Dispose();
            _runtimeStorage.Dispose();
        }
    }
    
}