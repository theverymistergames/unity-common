using MisterGames.Interact.Core;
using UnityEngine;
using UnityEngine.Events;

namespace MisterGames.Interact.Objects {

    [RequireComponent(typeof(Interactive))]
    public class DbgInteractiveUnityEvent : MonoBehaviour {

        [SerializeField] private UnityEvent _event;

        private Interactive _interactive;

        private void Awake() {
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
