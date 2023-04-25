using System;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Input {

    public interface ICharacterInputPipeline : ICharacterPipeline {

        event Action<Vector2> OnViewVectorChanged;
        event Action<Vector2> OnMotionVectorChanged;

        event Action RunPressed;
        event Action RunReleased;

        bool IsRunPressed { get; }
        bool WasRunPressed { get; }
        bool WasRunReleased { get; }

        event Action CrouchPressed;
        event Action CrouchReleased;
        event Action CrouchToggled;

        bool IsCrouchPressed { get; }
        bool WasCrouchPressed { get; }
        bool WasCrouchReleased { get; }
        bool WasCrouchToggled { get; }

        event Action JumpPressed;
    }

}
