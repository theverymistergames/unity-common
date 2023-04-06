using MisterGames.Character.Core2.Input;
using MisterGames.Character.Core2.Jump;
using MisterGames.Character.Core2.Motion;
using MisterGames.Character.Core2.Run;
using MisterGames.Character.Core2.View;
using MisterGames.Collisions.Core;
using MisterGames.UI.Initialization;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public sealed class CharacterAccess : MonoBehaviour, ICharacterAccess {

        [SerializeField] private CharacterInput _input;
        [SerializeField] private CameraController _cameraController;

        [SerializeField] private CharacterHeadAdapter headAdapter;
        [SerializeField] private CharacterBodyAdapter _motionAdapter;

        [SerializeField] private CharacterViewPipeline _viewPipeline;
        [SerializeField] private CharacterMotionPipeline _motionPipeline;
        [SerializeField] private CharacterJumpPipeline _jumpPipeline;
        [SerializeField] private CharacterRunPipeline _runPipeline;

        [SerializeField] private CollisionDetectorBase _hitDetector;
        [SerializeField] private CollisionDetectorBase _ceilingDetector;
        [SerializeField] private CollisionDetectorBase _groundDetector;

        public ICharacterInput Input => _input;
        public CameraController CameraController => _cameraController;

        public ITransformAdapter HeadAdapter => headAdapter;
        public ITransformAdapter BodyAdapter => _motionAdapter;

        public ICharacterViewPipeline ViewPipeline => _viewPipeline;
        public ICharacterMotionPipeline MotionPipeline => _motionPipeline;
        public ICharacterJumpPipeline JumpPipeline => _jumpPipeline;
        public ICharacterRunPipeline RunPipeline => _runPipeline;

        public ICollisionDetector HitDetector => _hitDetector;
        public ICollisionDetector CeilingDetector => _ceilingDetector;
        public ICollisionDetector GroundDetector => _groundDetector;

        private void Awake() {
            CanvasRegistry.Instance.SetCanvasEventCamera(_cameraController.Camera);
        }

        private void OnDestroy() {
            CanvasRegistry.Instance.SetCanvasEventCamera(null);
        }
    }

}
