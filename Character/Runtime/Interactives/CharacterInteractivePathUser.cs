using MisterGames.Actors;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Character.Interactives {

    public sealed class CharacterInteractivePathUser : MonoBehaviour, IActorComponent {

        private IActor _actor;
        private IInteractiveUser _user;

        public void OnAwake(IActor actor) {
            _user = actor.GetComponent<CharacterInteractionPipeline>().InteractiveUser;   
        }

        private void OnEnable() {
            _user.OnStartInteract -= OnStartInteract;
            _user.OnStartInteract += OnStartInteract;

            _user.OnStopInteract -= OnStopInteract;
            _user.OnStopInteract += OnStopInteract;
        }

        private void OnDisable() {
            _user.OnStartInteract -= OnStartInteract;
            _user.OnStopInteract -= OnStopInteract;
        }

        private void OnStartInteract(IInteractive interactive) {
            if (!interactive.Transform.TryGetComponent(out CharacterInteractivePathAdapter adapter)) return;
            
            adapter.AttachToPath(_actor, _user, interactive);
        }

        private void OnStopInteract(IInteractive interactive) {
            if (!interactive.Transform.TryGetComponent(out CharacterInteractivePathAdapter adapter)) return;
            
            adapter.DetachFromPath(_actor, _user, interactive);
        }
    }

}
