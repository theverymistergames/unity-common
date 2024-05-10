using MisterGames.Interact.Detectables;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Character.Interactives {

    public sealed class CharacterInteractionPipeline : MonoBehaviour {

        [SerializeField] private Detector _detector;
        [SerializeField] private InteractiveUser _interactiveUser;

        public IInteractiveUser InteractiveUser => _interactiveUser;

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void SetEnabled(bool isEnabled) {
            _detector.enabled = isEnabled;
            _interactiveUser.enabled = isEnabled;
        }
    }

}
