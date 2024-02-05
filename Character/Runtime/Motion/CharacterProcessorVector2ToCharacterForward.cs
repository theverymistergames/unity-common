using System;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Character.Processors;
using MisterGames.Collisions.Core;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.Motion {

    [Serializable]
    public sealed class CharacterProcessorVector2ToCharacterForward : ICharacterProcessorVector2ToVector3, ICharacterAccessInitializable {

        private ITransformAdapter _headAdapter;
        private ITransformAdapter _bodyAdapter;
        private ICollisionDetector _groundDetector;
        private CharacterProcessorMass _mass;
        private Func<Vector2, Vector3> _converter;

        public void Initialize(ICharacterAccess characterAccess) {
            _headAdapter = characterAccess.HeadAdapter;
            _bodyAdapter = characterAccess.BodyAdapter;

            _groundDetector = characterAccess.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;
            _mass = characterAccess.GetPipeline<ICharacterMotionPipeline>().GetProcessor<CharacterProcessorMass>();
        }

        public void DeInitialize() { }

        public void SetOverride(Func<Vector2, Vector3> converter) {
            _converter = converter;
        }

        public Vector3 Process(Vector2 input, float dt) {
            if (_converter != null) return _converter.Invoke(input);

            var dir = new Vector3(input.x, 0f, input.y);

            _groundDetector.FetchResults();
            var info = _groundDetector.CollisionInfo;

            // Gravity is enabled and has ground contact: use character body direction and consider ground normal
            if (_mass.isGravityEnabled && info.hasContact) {
                return Vector3.ProjectOnPlane(_bodyAdapter.Rotation * dir, info.normal);
            }

            // Move direction is same as view direction while gravity is not enabled or has no ground contact
            return _headAdapter.Rotation * dir;
        }
    }

}
