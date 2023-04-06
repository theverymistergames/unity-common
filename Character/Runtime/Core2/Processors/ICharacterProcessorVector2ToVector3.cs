using UnityEngine;

namespace MisterGames.Character.Core2.Processors {

    public interface ICharacterProcessorVector2ToVector3 {
        Vector3 Process(Vector2 input, float dt);
    }

}
