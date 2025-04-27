using UnityEngine;

namespace MisterGames.UI.Services {

    public sealed class CanvasRegister : MonoBehaviour {

        [SerializeField] private Canvas _canvas;

        private void Awake() {
            CanvasRegistry.Instance.AddCanvas(_canvas);
        }

        private void OnDestroy() {
            CanvasRegistry.Instance.RemoveCanvas(_canvas);
        }

#if UNITY_EDITOR
        private void Reset() {
            _canvas = GetComponent<Canvas>();
        }
#endif
    }
    
}
