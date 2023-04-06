using UnityEngine;

namespace MisterGames.Character.Core2.Processors {

    public interface ICharacterProcessorVector2ToQuaternion {
        Quaternion Process(Vector2 input, float dt);
    }

}
