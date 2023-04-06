using UnityEngine;

namespace MisterGames.Character.Core2.Jump {

    public interface ICharacterJumpPipeline {
        Vector3 Direction { get; set; }

        float Force { get; set; }
        float ForceMultiplier { get; set; }

        void SetEnabled(bool isEnabled);
    }

}
