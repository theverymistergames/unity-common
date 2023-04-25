using UnityEngine;

namespace MisterGames.Character.Processors {

    public interface ICharacterProcessorVector2 : ICharacterProcessor {
        Vector2 Process(Vector2 input, float dt);
    }

}
