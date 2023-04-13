using UnityEngine;

namespace MisterGames.Character.Processors {

    public interface ICharacterProcessorVector3 {
        Vector3 Process(Vector3 input, float dt);
    }
}
