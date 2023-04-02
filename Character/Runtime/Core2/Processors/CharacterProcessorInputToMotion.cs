using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    [Serializable]
    public sealed class CharacterProcessorInputToMotion : ICharacterProcessorVector2ToVector3, ICharacterProcessorInitializable {

        private ICharacterAccess _characterAccess;

        public void Initialize(ICharacterAccess characterAccess) {
            _characterAccess = characterAccess;
        }

        public void DeInitialize() { }

        public Vector3 Process(Vector2 input, float dt) {
            var dir = new Vector3(input.x, 0f, input.y);

            // Consider body rotation
            dir = _characterAccess.MotionAdapter.Rotation * dir;

            // Consider ground normal
            var groundDetector = _characterAccess.GroundDetector;
            groundDetector.FetchResults();

            var groundInfo = groundDetector.CollisionInfo;
            if (groundInfo.hasContact) dir = Quaternion.FromToRotation(Vector3.up, groundInfo.lastNormal) * dir;

            return dir;
        }
    }

}
