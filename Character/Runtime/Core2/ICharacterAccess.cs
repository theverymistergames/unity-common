using MisterGames.Collisions.Core;

namespace MisterGames.Character.Core2 {

    public interface ICharacterAccess {
        ICharacterInput Input { get; }

        ICharacterMotionAdapter MotionAdapter { get; }
        ICharacterMotionPipeline MotionPipeline { get; }

        ICollisionDetector HitDetector { get; }
        ICollisionDetector CeilingDetector { get; }
        ICollisionDetector GroundDetector { get; }
    }
}
