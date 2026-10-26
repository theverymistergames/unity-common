using MisterGames.Character.Core;
using MisterGames.Logic.Rendering;
using UnityEngine;

namespace MisterGames.Character.Behaviours {
    
    [DefaultExecutionOrder(-9000)]
    public sealed class CharacterCameraProvider : CameraProviderBehaviour {

        private Camera _camera;
        
        private void OnEnable() {
            _camera = CharacterSpawner.Instance.GetCharacter().GetComponent<Camera>();
        }

        private void OnDisable() {
            _camera = null;
        }
        
        public override Camera GetCamera() {
            return _camera;
        }
    }
    
}