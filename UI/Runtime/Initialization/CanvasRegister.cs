using UnityEngine;

namespace MisterGames.UI.Initialization {

    public class CanvasRegister : MonoBehaviour {

        [SerializeField] private Canvas _canvas;

        private void Start() {
            CanvasRegistry.Instance.AddCanvas(_canvas);
        }

        private void OnDestroy() {
            CanvasRegistry.Instance.RemoveCanvas(_canvas);
        }
    }
}
