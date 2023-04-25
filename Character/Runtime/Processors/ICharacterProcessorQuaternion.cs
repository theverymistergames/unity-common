using UnityEngine;

namespace MisterGames.Character.Processors {

    public interface ICharacterProcessorQuaternion : ICharacterProcessor {
        Quaternion Process(Quaternion input, float dt);
    }

}
