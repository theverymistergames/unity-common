using System;
using UnityEngine;

namespace MisterGames.Character.Core2.Jump {

    public interface ICharacterJumpPipeline {
        event Action<Vector3> OnJump;

        Vector3 Direction { get; set; }
        float Force { get; set; }
        float ForceMultiplier { get; }

        void SetForceMultiplier(object source, float multiplier);
        void ResetForceMultiplier(object source);

        void SetEnabled(bool isEnabled);
    }

}
