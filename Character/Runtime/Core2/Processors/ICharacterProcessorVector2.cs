using UnityEngine;

namespace MisterGames.Character.Core2 {

    public interface ICharacterProcessorVector2 {
        Vector2 Process(Vector2 input, float dt);
    }

}
