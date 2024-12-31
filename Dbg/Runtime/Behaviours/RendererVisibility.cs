using UnityEngine;

namespace MisterGames.Dbg.Behaviours {
    
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer))]
    public sealed class RendererVisibility : MonoBehaviour {

        [SerializeField] private bool _visibleInEditMode = true;
        [SerializeField] private bool _visibleInPlayMode = false;

        private void Awake() {
            UpdateVisibility();
        }

        private void OnValidate() {
            UpdateVisibility();
        }

        private void Reset() {
            UpdateVisibility();
        }

        private void UpdateVisibility() {
            if (!TryGetComponent(out Renderer renderer)) return;

            renderer.enabled = Application.isPlaying ? _visibleInPlayMode : _visibleInEditMode;
        }
    }
    
}