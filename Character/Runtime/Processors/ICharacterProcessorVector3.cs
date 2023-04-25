using UnityEngine;

namespace MisterGames.Character.Processors {

    public interface ICharacterProcessorVector3 : ICharacterProcessor {
        Vector3 Process(Vector3 input, float dt);
    }
}
