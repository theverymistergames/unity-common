using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Character.Core {
    
    [DefaultExecutionOrder(-10000)]
    public sealed class CharacterServiceLauncher : MonoBehaviour {
        
        private readonly CharacterSettings _characterSettings = new();
        
        private void Awake() {
            Services.Register<ICharacterSettings>(_characterSettings);
        }

        private void OnDestroy() {
            Services.Unregister(_characterSettings);
        }
    }
    
}