using System;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Input {

    public interface ICharacterInputPipeline : ICharacterPipeline {

        event Action<Vector2> OnViewVectorChanged;
        event Action<Vector2> OnMotionVectorChanged;

        event Action OnRunPressed;
        event Action OnRunReleased;

        event Action OnCrouchPressed;
        event Action OnCrouchReleased;
        event Action OnCrouchToggled;

        event Action JumpPressed;

        bool IsRunPressed { get; }

        bool IsCrouchPressed { get; }
        bool WasCrouchToggled { get; }

        void EnableViewInput(bool enable);
    }

}
