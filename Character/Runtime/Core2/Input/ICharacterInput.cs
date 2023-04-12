using System;
using UnityEngine;

namespace MisterGames.Character.Core2.Input {

    public interface ICharacterInput {
        event Action<Vector2> OnViewVectorChanged;
        event Action<Vector2> OnMotionVectorChanged;

        event Action RunToggled;
        bool WasRunToggled { get; }

        event Action CrouchPressed;
        event Action CrouchReleased;
        event Action CrouchToggled;

        bool IsCrouchInputActive { get; }
        bool WasCrouchPressed { get; }
        bool WasCrouchReleased { get; }
        bool WasCrouchToggled { get; }

        event Action JumpPressed;
        bool IsJumpPressed { get; }

        void SetEnabled(bool isEnabled);
    }

}
