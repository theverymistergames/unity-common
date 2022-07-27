using MisterGames.Input.Actions;
using MisterGames.Input.Core;
using MisterGames.Interact.Cursors;
using UnityEngine;

namespace MisterGames.Interact.Core {
    
    [CreateAssetMenu(fileName = nameof(InteractStrategy), menuName = "MisterGames/Interact/" + nameof(InteractStrategy))]
    public sealed class InteractStrategy : ScriptableObject {
        
        [Header("Input Settings")]
        public InputActionKey inputAction;
        public InteractiveMode mode;

        [Header("Modifiers")]
        public InputActionFilter filter;
        public CursorIcon cursorIconHover;
        public CursorIcon cursorIconInteract;

        [Header("Conditions")]
        public bool stopInteractWhenExceededMaxDistance;
        public bool stopInteractWhenNotInView;
        public float maxInteractDistance;
    }

}
