using MisterGames.Character.Collisions;
using MisterGames.Character.Fsm;
using MisterGames.Character.Height;
using MisterGames.Character.Input;
using MisterGames.Character.Jump;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Collisions.Core;
using MisterGames.Common.GameObjects;
using MisterGames.UI.Initialization;
using UnityEngine;

namespace MisterGames.Character.Access {

    public sealed class CharacterAccess : MonoBehaviour, ICharacterAccess {

        [SerializeField] private CharacterInput _input;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private CharacterController _characterController;

        [SerializeField] private CharacterHeadAdapter headAdapter;
        [SerializeField] private CharacterBodyAdapter _bodyAdapter;

        [SerializeField] private CharacterViewPipeline _viewPipeline;
        [SerializeField] private CharacterMotionPipeline _motionPipeline;
        [SerializeField] private CharacterMotionFsmPipeline _motionFsmPipeline;
        [SerializeField] private CharacterJumpPipeline _jumpPipeline;
        [SerializeField] private CharacterHeightPipeline _heightPipeline;

        [SerializeField] private CollisionDetectorBase _hitDetector;
        [SerializeField] private CharacterCeilingDetector _ceilingDetector;
        [SerializeField] private CharacterGroundDetector _groundDetector;

        public ICharacterInput Input => _input;
        public CameraController CameraController => _cameraController;
        public CharacterController CharacterController => _characterController;

        public ITransformAdapter HeadAdapter => headAdapter;
        public ITransformAdapter BodyAdapter => _bodyAdapter;

        public ICharacterViewPipeline ViewPipeline => _viewPipeline;
        public ICharacterMotionPipeline MotionPipeline => _motionPipeline;
        public ICharacterMotionFsmPipeline MotionFsmPipeline => _motionFsmPipeline;
        public ICharacterJumpPipeline JumpPipeline => _jumpPipeline;
        public ICharacterHeightPipeline HeightPipeline => _heightPipeline;

        public ICollisionDetector HitDetector => _hitDetector;
        public CharacterCeilingDetector CeilingDetector => _ceilingDetector;
        public CharacterGroundDetector GroundDetector => _groundDetector;

        private void Awake() {
            CanvasRegistry.Instance.SetCanvasEventCamera(_cameraController.Camera);
        }

        private void OnDestroy() {
            CanvasRegistry.Instance.SetCanvasEventCamera(null);
        }
    }

}
