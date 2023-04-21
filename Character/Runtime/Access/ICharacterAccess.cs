using MisterGames.Character.Collisions;
using MisterGames.Character.Fsm;
using MisterGames.Character.Height;
using MisterGames.Character.Input;
using MisterGames.Character.Jump;
using MisterGames.Character.View;
using MisterGames.Collisions.Core;
using MisterGames.Common.GameObjects;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Character.Access {

    public interface ICharacterAccess {
        ICharacterInput Input { get; }
        CameraController CameraController { get; }
        CharacterController CharacterController { get; }
        IInteractiveUser InteractiveUser { get; }

        ITransformAdapter HeadAdapter { get; }
        ITransformAdapter BodyAdapter { get; }

        ICharacterViewPipeline ViewPipeline { get; }
        ICharacterMotionPipeline MotionPipeline { get; }
        ICharacterMotionFsmPipeline MotionFsmPipeline { get; }
        ICharacterJumpPipeline JumpPipeline { get; }
        ICharacterHeightPipeline HeightPipeline { get; }

        ICollisionDetector HitDetector { get; }
        CharacterCeilingDetector CeilingDetector { get; }
        CharacterGroundDetector GroundDetector { get; }
    }
}
