using MisterGames.Character.Core;
using MisterGames.Common.GameObjects;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Character.Interactives {

    public sealed class CharacterInteractivePathUser : MonoBehaviour {

        [SerializeField] private CharacterAccess _characterAccess;

        private IInteractiveUser _user;
        private ITransformAdapter _body;

        private void Awake() {
            _user = _characterAccess.GetPipeline<ICharacterInteractionPipeline>().InteractiveUser;
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
            interactive.Transform
                .GetComponent<ICharacterInteractivePath>()
                ?.AttachToPath(_characterAccess, _user, interactive);
        }

        private void OnStopInteract(IInteractive interactive) {
            interactive.Transform
                .GetComponent<ICharacterInteractivePath>()
                ?.DetachFromPath(_characterAccess, _user, interactive);
        }
    }

}
