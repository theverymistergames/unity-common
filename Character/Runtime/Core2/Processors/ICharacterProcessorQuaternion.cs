using UnityEngine;

namespace MisterGames.Character.Core2.Processors {

    public interface ICharacterProcessorQuaternion {
        Quaternion Process(Quaternion input, float dt);
    }

}
