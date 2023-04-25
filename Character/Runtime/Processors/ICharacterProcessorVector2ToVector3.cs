using UnityEngine;

namespace MisterGames.Character.Processors {

    public interface ICharacterProcessorVector2ToVector3 : ICharacterProcessor {
        Vector3 Process(Vector2 input, float dt);
    }

}
