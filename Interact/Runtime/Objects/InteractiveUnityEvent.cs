using System;
using MisterGames.Interact.Interactives;
using UnityEngine;
using UnityEngine.Events;

namespace MisterGames.Interact.Objects {

    [RequireComponent(typeof(Interactive))]
    [Obsolete("InteractiveUnityEvent must be replaced later!")]
    public class InteractiveUnityEvent : MonoBehaviour {

        [SerializeField] private UnityEvent _event;

        private IInteractive _interactive;

        private void Awake() {
            Debug.LogWarning($"Using {nameof(InteractiveUnityEvent)} on game object `{gameObject.name}`. " +
                             $"It must be replaced later!");

            _interactive = GetComponent<IInteractive>();
        }

        private void OnEnable() {
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStartInteract += OnStartInteract;
        }

        private void OnDisable() {
            _interactive.OnStartInteract -= OnStartInteract;
        }

        private void OnStartInteract(IInteractiveUser user) {
            _event.Invoke();
        }
    }
}
