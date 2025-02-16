using MisterGames.Actors;
using MisterGames.Character.Motion;
using MisterGames.Interact.Detectables;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Character.Interactives {

    public sealed class CharacterInteractionPipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private Detector _detector;
        [SerializeField] private InteractiveUser _interactiveUser;

        public IInteractiveUser InteractiveUser => _interactiveUser;

        private CharacterMotionPipeline _motion;
        
        void IActorComponent.OnAwake(IActor actor) {
            _motion = actor.GetComponent<CharacterMotionPipeline>();
        }

        private void OnEnable() {
            SetEnabled(true);
            
            _motion.OnTeleport += OnTeleport;
        }

        private void OnDisable() {
            SetEnabled(false);
            
            _motion.OnTeleport -= OnTeleport;
        }

        private void OnTeleport() {
            _interactiveUser.ForceStopInteractAll();
        }

        private void SetEnabled(bool isEnabled) {
            _detector.enabled = isEnabled;
            _interactiveUser.enabled = isEnabled;
        }
    }

}
