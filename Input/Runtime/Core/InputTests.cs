using MisterGames.Common.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    public class InputTests : MonoBehaviour {

        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private InputActionRef _inputActionRef;
        [SerializeField] private InputMapRef _inputMapRef;

        [Button]
        private void Test() {
            
        }
    }
}