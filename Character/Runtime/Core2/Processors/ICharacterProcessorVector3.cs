using UnityEngine;

namespace MisterGames.Character.Core2 {

    public interface ICharacterProcessorVector3 {
        Vector3 Process(Vector3 input, float dt);
    }
}
