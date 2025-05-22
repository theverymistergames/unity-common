using MisterGames.Character.Core;
using MisterGames.Character.View;
using UnityEngine;

namespace MisterGames.Character.Behaviours {
    
    public sealed class CharacterCameraCullingMaskOverride : MonoBehaviour {
        
        [SerializeField] private LayerMask _cullingMask;
        [SerializeField] private CameraContainer.MaskMode _maskMode;

        private int _stateId;
        
        private void OnEnable() {
            var cameraContainer = CharacterSystem.Instance.GetCharacter().GetComponent<CameraContainer>();
            _stateId = cameraContainer.CreateState();
            
            cameraContainer.SetCullingMask(_stateId, _cullingMask, _maskMode);
        }

        private void OnDisable() {
            var cameraContainer = CharacterSystem.Instance.GetCharacter().GetComponent<CameraContainer>();
            cameraContainer.RemoveCullingMask(_stateId);
        }
    }
    
}