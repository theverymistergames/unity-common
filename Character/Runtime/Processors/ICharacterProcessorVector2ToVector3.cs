using UnityEngine;

namespace MisterGames.Character.Processors {

    public interface ICharacterProcessorVector2ToVector3 {
        Vector3 Process(Vector2 input, float dt);
    }

}
