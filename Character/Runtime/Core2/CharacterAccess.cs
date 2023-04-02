using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public sealed class CharacterAccess : MonoBehaviour, ICharacterAccess {

        [SerializeField] private Transform _head;
        [SerializeField] private Transform _body;

        [SerializeField] private CharacterInput _input;

        [SerializeField] private CharacterMotionAdapter _motionAdapter;
        [SerializeField] private CharacterMotionPipeline _motionPipeline;

        [SerializeField] private CollisionDetectorBase _hitDetector;
        [SerializeField] private CollisionDetectorBase _ceilingDetector;
        [SerializeField] private CollisionDetectorBase _groundDetector;

        public Transform Head => _head;
        public Transform Body => _body;

        public ICharacterInput Input => _input;

        public ICharacterMotionAdapter MotionAdapter => _motionAdapter;
        public ICharacterMotionPipeline MotionPipeline => _motionPipeline;

        public ICollisionDetector HitDetector => _hitDetector;
        public ICollisionDetector CeilingDetector => _ceilingDetector;
        public ICollisionDetector GroundDetector => _groundDetector;
    }

}
