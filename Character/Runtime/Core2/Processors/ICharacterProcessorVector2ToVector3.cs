using UnityEngine;

namespace MisterGames.Character.Core2 {

    public interface ICharacterProcessorVector2ToVector3 {
        Vector3 Process(Vector2 input, float dt);
    }

}
