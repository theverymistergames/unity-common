using MisterGames.Character.Core2.Input;
using MisterGames.Character.Core2.Jump;
using MisterGames.Character.Core2.Run;
using MisterGames.Character.Core2.View;
using MisterGames.Collisions.Core;

namespace MisterGames.Character.Core2 {

    public interface ICharacterAccess {
        ICharacterInput Input { get; }
        CameraController CameraController { get; }

        ITransformAdapter HeadAdapter { get; }
        ITransformAdapter BodyAdapter { get; }

        ICharacterViewPipeline ViewPipeline { get; }
        ICharacterMotionPipeline MotionPipeline { get; }
        ICharacterJumpPipeline JumpPipeline { get; }
        ICharacterRunPipeline RunPipeline { get; }

        ICollisionDetector HitDetector { get; }
        ICollisionDetector CeilingDetector { get; }
        ICollisionDetector GroundDetector { get; }
    }
}
