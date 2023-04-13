using UnityEngine;

namespace MisterGames.Character.Processors {

    public interface ICharacterProcessorQuaternion {
        Quaternion Process(Quaternion input, float dt);
    }

}
