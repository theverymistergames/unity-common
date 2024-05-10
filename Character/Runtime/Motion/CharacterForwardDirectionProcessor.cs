using System;
using MisterGames.Actors;
using MisterGames.Character.Collisions;
using MisterGames.Character.View;
using MisterGames.Collisions.Core;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.Motion {

    [Serializable]
    public sealed class CharacterForwardDirectionProcessor {

        private ITransformAdapter _headAdapter;
        private ITransformAdapter _bodyAdapter;
        private ICollisionDetector _groundDetector;
        private CharacterMassProcessor _mass;
        private Func<Vector2, Vector3> _converter;

        public void Initialize(IActor actor) {
            _headAdapter = actor.GetComponent<CharacterHeadAdapter>();
            _bodyAdapter = actor.GetComponent<CharacterBodyAdapter>();
            _groundDetector = actor.GetComponent<CharacterCollisionPipeline>().GroundDetector;
            _mass = actor.GetComponent<CharacterMotionPipeline>().GetProcessor<CharacterMassProcessor>();
        }

        public void DeInitialize(IActor actor) { }

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
