using MisterGames.Interact.Core;
using UnityEngine;
using UnityEngine.Events;

namespace MisterGames.Interact.Objects {

    [RequireComponent(typeof(Interactive))]
    public class InteractiveUnityEvent : MonoBehaviour {

        [SerializeField] private UnityEvent _event;

        private Interactive _interactive;

        private void Awake() {
            Debug.LogWarning($"Using {nameof(InteractiveUnityEvent)} on game object `{gameObject.name}`. " +
                             $"It must be replaced later!");

            _interactive = GetComponent<Interactive>();
        }

        private void OnEnable() {
            _interactive.OnStartInteract += OnStartInteract;
        }

        private void OnDisable() {
            _interactive.OnStartInteract -= OnStartInteract;
        }

        private void OnStartInteract(InteractiveUser user) {
            _event.Invoke();
        }
    }
}
