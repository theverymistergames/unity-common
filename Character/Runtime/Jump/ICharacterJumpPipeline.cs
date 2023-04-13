using System;
using UnityEngine;

namespace MisterGames.Character.Jump {

    public interface ICharacterJumpPipeline {
        event Action<Vector3> OnJump;

        Vector3 Direction { get; set; }
        float Force { get; set; }
        float ForceMultiplier { get; set; }

        void SetEnabled(bool isEnabled);
    }

}
