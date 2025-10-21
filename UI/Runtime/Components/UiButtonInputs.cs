using MisterGames.Input.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.UI.Components {
    
    [RequireComponent(typeof(UiButton))]
    public sealed class UiButtonInputs : MonoBehaviour {

        [SerializeField] private UiButton _button;
        [SerializeField] private InputActionRef[] _inputs;

        private void OnEnable() {
            for (int i = 0; i < _inputs.Length; i++) {
                if (_inputs[i].Get() is {} input) input.performed += OnPerformInput;
            }
        }

        private void OnDisable() {
            for (int i = 0; i < _inputs.Length; i++) {
                if (_inputs[i].Get() is {} input) input.performed -= OnPerformInput;
            }
        }

        private void OnPerformInput(InputAction.CallbackContext obj) {
            _button.ClickManual();
        }

#if UNITY_EDITOR
        private void Reset() {
            _button = GetComponent<UiButton>();
        }
#endif
    }
    
}