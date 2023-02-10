using UnityEngine;

namespace MisterGames.Input.Core {

    public class InputRunner : MonoBehaviour {

        [SerializeField] private InputUpdater _inputUpdater;

        public void Awake() {
            _inputUpdater.Awake();
        }

        public void OnDestroy() {
            _inputUpdater.OnDestroy();
        }

        public void OnEnable() {
            _inputUpdater.OnEnable();
        }

        public void OnDisable() {
            _inputUpdater.OnDisable();
        }
    }

}
