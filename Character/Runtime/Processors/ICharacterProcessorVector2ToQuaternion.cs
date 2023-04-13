using UnityEngine;

namespace MisterGames.Character.Processors {

    public interface ICharacterProcessorVector2ToQuaternion {
        Quaternion Process(Vector2 input, float dt);
    }

}
