using UnityEngine;

namespace MisterGames.Common.Labels {
    
    [DefaultExecutionOrder(-100_000)]
    internal sealed class LabelValueEventSystemRunner : MonoBehaviour {

        public static ILabelValueEventSystem EventSystem => _eventSystem;
        
        private static readonly LabelValueEventSystem _eventSystem = new();

        private void OnDestroy() {
            _eventSystem.Clear();
        }
    }
    
}