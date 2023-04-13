using UnityEngine;

namespace MisterGames.Character {

    public interface ICharacterMotionPipeline {
        Vector2 MotionInput { get; }

        P GetProcessor<P>() where P : class;
        void SetEnabled(bool isEnabled);
    }

}
