using MisterGames.Collisions.Core;

namespace MisterGames.Character.Core2 {

    public interface ICharacterAccess {
        ICharacterInput Input { get; }
        CameraController CameraController { get; }

        ITransformAdapter ViewAdapter { get; }
        ITransformAdapter MotionAdapter { get; }

        ICharacterPipeline ViewPipeline { get; }
        ICharacterPipeline MotionPipeline { get; }

        ICollisionDetector HitDetector { get; }
        ICollisionDetector CeilingDetector { get; }
        ICollisionDetector GroundDetector { get; }

        ICharacterJumpProcessor JumpProcessor { get; }
    }
}
