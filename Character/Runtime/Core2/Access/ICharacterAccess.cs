using MisterGames.Character.Core2.Collisions;
using MisterGames.Character.Core2.Height;
using MisterGames.Character.Core2.Input;
using MisterGames.Character.Core2.Jump;
using MisterGames.Character.Core2.View;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public interface ICharacterAccess {
        ICharacterInput Input { get; }
        CameraController CameraController { get; }
        CharacterController CharacterController { get; }

        ITransformAdapter HeadAdapter { get; }
        ITransformAdapter BodyAdapter { get; }

        ICharacterViewPipeline ViewPipeline { get; }
        ICharacterMotionPipeline MotionPipeline { get; }
        ICharacterJumpPipeline JumpPipeline { get; }
        ICharacterHeightPipeline HeightPipeline { get; }

        ICollisionDetector HitDetector { get; }
        CharacterCeilingDetector CeilingDetector { get; }
        CharacterGroundDetector GroundDetector { get; }
    }
}
