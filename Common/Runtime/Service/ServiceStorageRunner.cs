using UnityEngine;

namespace MisterGames.Common.Service {
    
    internal sealed class ServiceStorageRunner : MonoBehaviour {

        public static readonly ServiceStorage ServiceStorage = new();

        private void OnDestroy() {
            ServiceStorage.Clear();
        }
    }
    
}