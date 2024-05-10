using UnityEngine;

namespace MisterGames.Character.Processors {

    public interface ICharacterProcessorVector2 {
        Vector2 Process(Vector2 input, float dt);
    }

}
