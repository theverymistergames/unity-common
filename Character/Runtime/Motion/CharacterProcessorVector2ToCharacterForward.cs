using System;
using MisterGames.Character.Access;
using MisterGames.Character.Processors;
using MisterGames.Collisions.Core;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.Motion {

    [Serializable]
    public sealed class CharacterProcessorVector2ToCharacterForward : ICharacterProcessorVector2ToVector3, ICharacterProcessorInitializable {

        private ITransformAdapter _headAdapter;
        private ITransformAdapter _bodyAdapter;
        private ICollisionDetector _groundDetector;
        private CharacterProcessorMass _mass;

        public void Initialize(ICharacterAccess characterAccess) {
            _headAdapter = characterAccess.HeadAdapter;
            _bodyAdapter = characterAccess.BodyAdapter;

            _groundDetector = characterAccess.GroundDetector;
            _mass = characterAccess.MotionPipeline.GetProcessor<CharacterProcessorMass>();
        }

        public void DeInitialize() { }

        public Vector3 Process(Vector2 input, float dt) {
            var dir = new Vector3(input.x, 0f, input.y);

            _groundDetector.FetchResults();
            var groundInfo = _groundDetector.CollisionInfo;

            // Consider rotation depending on gravity enable state
            dir = _bodyAdapter.Rotation * dir;

            // Move direction is same as view direction while gravity is not enabled
            if (!groundInfo.hasContact && !_mass.isGravityEnabled) dir = _headAdapter.Rotation * dir;

            // Consider ground normal
            if (groundInfo.hasContact) dir = Vector3.ProjectOnPlane(dir, groundInfo.normal);

            return dir;
        }
    }

}
