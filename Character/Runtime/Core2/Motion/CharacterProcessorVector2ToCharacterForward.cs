using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Character.Core2.Processors;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Character.Core2.Motion {

    [Serializable]
    public sealed class CharacterProcessorVector2ToCharacterForward : ICharacterProcessorVector2ToVector3, ICharacterProcessorInitializable {

        private ITransformAdapter _bodyAdapter;
        private ICollisionDetector _groundDetector;

        public void Initialize(ICharacterAccess characterAccess) {
            _bodyAdapter = characterAccess.BodyAdapter;
            _groundDetector = characterAccess.GroundDetector;
        }

        public void DeInitialize() { }

        public Vector3 Process(Vector2 input, float dt) {
            var dir = new Vector3(input.x, 0f, input.y);

            // Consider body rotation
            dir = _bodyAdapter.Rotation * dir;

            // Consider ground normal
            _groundDetector.FetchResults();
            var groundInfo = _groundDetector.CollisionInfo;
            if (groundInfo.hasContact) dir = Vector3.ProjectOnPlane(dir, groundInfo.lastNormal);

            return dir;
        }
    }

}
