using System;
using MisterGames.Character.Access;
using MisterGames.Character.Processors;
using MisterGames.Collisions.Core;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.Motion {

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
            if (groundInfo.hasContact) dir = Vector3.ProjectOnPlane(dir, groundInfo.normal);

            return dir;
        }
    }

}
