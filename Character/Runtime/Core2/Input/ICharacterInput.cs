﻿using System;
using UnityEngine;

namespace MisterGames.Character.Core2.Input {

    public interface ICharacterInput {
        event Action<Vector2> OnViewVectorChanged;
        event Action<Vector2> OnMotionVectorChanged;

        event Action RunPressed;
        event Action RunReleased;
        bool IsRunPressed { get; }

        event Action CrouchPressed;
        event Action CrouchReleased;
        event Action CrouchToggled;
        bool IsCrouchPressed { get; }

        event Action JumpPressed;
        bool IsJumpPressed { get; }
    }

}