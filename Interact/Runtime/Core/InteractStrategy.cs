using MisterGames.Common.Attributes;
using MisterGames.Input.Actions;
using MisterGames.Input.Bindings;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.Interact.Core {
    
    [CreateAssetMenu(fileName = nameof(InteractStrategy), menuName = "MisterGames/Interact/" + nameof(InteractStrategy))]
    public sealed class InteractStrategy : ScriptableObject {
        
        [Header("Input Settings")]
        public InputActionKey inputAction;
        public Mode mode;
        
        [Header("Modifiers")]
        [SerializeField] private InputActionFilter _filter;

        [Header("Conditions")]
        public bool stopInteractWhenExceededMaxDistance;
        public bool stopInteractWhenNotInView;
        public float maxInteractDistance;

        public void Apply(InteractiveUser user) {
            _filter.Apply();
        }

        public void Release(InteractiveUser user) {
            _filter.Release();
        }
        
        public enum Mode {
            Tap,
            WhilePressed,
            ClickOnOff,
        }

    }
    
}
