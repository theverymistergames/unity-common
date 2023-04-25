using System;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Jump {

    public interface ICharacterJumpPipeline : ICharacterPipeline {
        event Action<Vector3> OnJump;

        Vector3 Direction { get; set; }
        float Force { get; set; }
        float ForceMultiplier { get; set; }
    }

}
