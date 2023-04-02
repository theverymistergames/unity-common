using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public interface ICharacterInput {
        event Action<Vector2> View;
        event Action<Vector2> Move;
        event Action StartRun;
        event Action StopRun;
        event Action ToggleRun;
        event Action StartCrouch;
        event Action StopCrouch;
        event Action ToggleCrouch;
        event Action Jump;
    }

}
