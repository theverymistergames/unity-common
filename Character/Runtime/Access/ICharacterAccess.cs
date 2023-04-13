using MisterGames.Character.Collisions;
using MisterGames.Character.Height;
using MisterGames.Character.Input;
using MisterGames.Character.Jump;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Character.Access {

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
