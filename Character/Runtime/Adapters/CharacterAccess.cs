using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.UI.Initialization;
using UnityEngine;

namespace MisterGames.Character.Adapters {
    
    public class CharacterAccess : MonoBehaviour {

        [SerializeField] private CharacterAdapter _characterAdapter;
        [SerializeField] private CameraController _cameraController;

        public static CharacterAccess Instance { get; private set; }

        public CharacterAdapter CharacterAdapter => _characterAdapter;

        private void Awake() {
            Instance = this;
            CanvasRegistry.Instance.SetCanvasEventCamera(_cameraController.Camera);
        }

        private void OnDestroy() {
            Instance = null;
            CanvasRegistry.Instance.SetCanvasEventCamera(null);
        }
    }
    
}
