using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.UI.Service {

    public sealed class CanvasRegister : MonoBehaviour {

        [SerializeField] private Canvas _canvas;

        private void Awake() {
            Services.Get<CanvasRegistry>()?.AddCanvas(_canvas);
        }

        private void OnDestroy() {
            Services.Get<CanvasRegistry>()?.RemoveCanvas(_canvas);
        }

#if UNITY_EDITOR
        private void Reset() {
            _canvas = GetComponent<Canvas>();
        }
#endif
    }
    
}
