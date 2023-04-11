using MisterGames.Character.Core2.Collisions;
using MisterGames.Character.Core2.Height;
using MisterGames.Character.Core2.Input;
using MisterGames.Character.Core2.Jump;
using MisterGames.Character.Core2.Motion;
using MisterGames.Character.Core2.View;
using MisterGames.Collisions.Core;
using MisterGames.UI.Initialization;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public sealed class CharacterAccess : MonoBehaviour, ICharacterAccess {

        [SerializeField] private CharacterInput _input;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private CharacterController _characterController;

        [SerializeField] private CharacterHeadAdapter headAdapter;
        [SerializeField] private CharacterBodyAdapter _bodyAdapter;

        [SerializeField] private CharacterViewPipeline _viewPipeline;
        [SerializeField] private CharacterMotionPipeline _motionPipeline;
        [SerializeField] private CharacterJumpPipeline _jumpPipeline;
        [SerializeField] private CharacterHeightPipeline _heightPipeline;

        [SerializeField] private CollisionDetectorBase _hitDetector;
        [SerializeField] private CollisionDetectorBase _ceilingDetector;
        [SerializeField] private CharacterGroundDetector _groundDetector;

        public ICharacterInput Input => _input;
        public CameraController CameraController => _cameraController;
        public CharacterController CharacterController => _characterController;

        public ITransformAdapter HeadAdapter => headAdapter;
        public ITransformAdapter BodyAdapter => _bodyAdapter;

        public ICharacterViewPipeline ViewPipeline => _viewPipeline;
        public ICharacterMotionPipeline MotionPipeline => _motionPipeline;
        public ICharacterJumpPipeline JumpPipeline => _jumpPipeline;
        public ICharacterHeightPipeline HeightPipeline => _heightPipeline;

        public ICollisionDetector HitDetector => _hitDetector;
        public ICollisionDetector CeilingDetector => _ceilingDetector;
        public CharacterGroundDetector GroundDetector => _groundDetector;

        private void Awake() {
            CanvasRegistry.Instance.SetCanvasEventCamera(_cameraController.Camera);
        }

        private void OnDestroy() {
            CanvasRegistry.Instance.SetCanvasEventCamera(null);
        }
    }

}
