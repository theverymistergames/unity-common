using UnityEngine;

namespace MisterGames.Character.Core2 {

    public interface ICharacterJumpProcessor {
        Vector3 Direction { get; set; }
        float Force { get; set; }

        void SetEnabled(bool isEnabled);
    }

}
