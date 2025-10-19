using MisterGames.Input.Actions;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.Input.Components {
    
    public sealed class InputBlocker : MonoBehaviour {

        [SerializeField] private InputMapRef[] _blockInputMaps;
        [SerializeField] private InputActionRef[] _overrideEnableInputActions;
        [SerializeField] private InputActionRef[] _overrideDisableInputActions;
        
        private void OnEnable() {
            Block();
        }

        private void OnDisable() {
            Unblock();
        }

        private void Block() {
            InputServices.Blocks.BlockInputMaps(this, _blockInputMaps);
            InputServices.Blocks.SetInputActionBlockOverrides(this, _overrideEnableInputActions, blocked: false);
            InputServices.Blocks.SetInputActionBlockOverrides(this, _overrideDisableInputActions, blocked: true);
        }

        private void Unblock() {
            InputServices.Blocks.ClearAllInputMapBlocksOf(this);
            InputServices.Blocks.ClearAllInputActionBlockOverridesOf(this);
        }
    }
    
}