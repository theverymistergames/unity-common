using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    public class InputTests : MonoBehaviour {

        [SerializeField] private InputActionAsset _inputActionAsset;

        [Button]
        private void Test() {
            Debug.Log($"InputTests.Test: f {Time.frameCount}, InputSystem.actions {InputSystem.actions}");
            
            EditorGUIUtility.PingObject(InputSystem.actions);
        }
    }
}