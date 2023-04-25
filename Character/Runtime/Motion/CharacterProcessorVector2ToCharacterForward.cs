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
            var groundInfo = _groundDetector.CollisionInfo;

            // Move direction is same as view direction while gravity is not enabled
            dir = (groundInfo.hasContact || _mass.isGravityEnabled ? _bodyAdapter.Rotation : _headAdapter.Rotation) * dir;

            // Consider ground normal
            if (groundInfo.hasContact) dir = Vector3.ProjectOnPlane(dir, groundInfo.normal);

            return dir;
        }
    }

}
