using UnityEngine;

namespace MisterGames.Character.Core2 {

    public interface ICharacterProcessorQuaternion {
        Quaternion Process(Quaternion input, float dt);
    }

}
