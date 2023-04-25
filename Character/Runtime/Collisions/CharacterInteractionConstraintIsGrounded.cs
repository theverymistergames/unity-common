using System;
using MisterGames.Character.Core;
using MisterGames.Character.Interactives;
using MisterGames.Collisions.Core;
using MisterGames.Interact.Interactives;

namespace MisterGames.Character.Collisions {

    [Serializable]
    public sealed class CharacterInteractionConstraintIsGrounded : IInteractionConstraint, ICharacterAccessInitializable {

        public bool shouldBeGrounded;

        private ICollisionDetector _groundDetector;
        private IInteractiveUser _interactiveUser;

        public void Initialize(ICharacterAccess characterAccess) {
            _interactiveUser = characterAccess.GetPipeline<ICharacterInteractionPipeline>().InteractiveUser;
            _groundDetector = characterAccess.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;
        }

        public void DeInitialize() { }

        public bool IsSatisfied(IInteractiveUser user, IInteractive interactive) {
            return _interactiveUser == user && shouldBeGrounded == _groundDetector.CollisionInfo.hasContact;
        }
    }

}
