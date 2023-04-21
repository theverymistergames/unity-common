    using System;
    using MisterGames.Character.Access;
    using MisterGames.Character.Motion;
    using MisterGames.Interact.Interactives;
using MisterGames.Interact.Path;
using UnityEngine;

namespace MisterGames.Character.Interactive {

    public sealed class CharacterInteractivePathUser : MonoBehaviour, IInteractivePathUser {

        [SerializeField] private CharacterAccess _characterAccess;

        private void OnEnable() {
            _characterAccess.MotionFsmPipeline.Register(this);
        }

        private void OnDisable() {
            _characterAccess.MotionFsmPipeline.Unregister(this);
        }

        public void OnAttachedToPath(IInteractiveUser user, IInteractivePath path, float t) {
            if (user != _characterAccess.InteractiveUser) return;

            _characterAccess.MotionFsmPipeline.SetEnabled(this, false);

            var mass = _characterAccess.MotionPipeline.GetProcessor<CharacterProcessorMass>();
            if (mass != null) {
                mass.isGravityEnabled = false;
                mass.ApplyVelocityChange(Vector3.zero);
            }
        }

        public void OnDetachedFromPath(IInteractiveUser user, IInteractivePath path) {
            if (user != _characterAccess.InteractiveUser) return;

            _characterAccess.MotionFsmPipeline.SetEnabled(this, true);

            var mass = _characterAccess.MotionPipeline.GetProcessor<CharacterProcessorMass>();
            if (mass != null) mass.isGravityEnabled = true;
        }
    }

}
