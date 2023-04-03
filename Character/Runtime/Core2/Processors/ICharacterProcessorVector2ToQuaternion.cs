using UnityEngine;

namespace MisterGames.Character.Core2 {

    public interface ICharacterProcessorVector2ToQuaternion {
        Quaternion Process(Vector2 input, float dt);
    }

}
