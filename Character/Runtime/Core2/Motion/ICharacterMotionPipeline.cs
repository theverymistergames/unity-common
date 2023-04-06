using UnityEngine;

namespace MisterGames.Character.Core2 {

    public interface ICharacterMotionPipeline {
        Vector2 MotionInput { get; }

        P GetProcessor<P>() where P : class;
        void SetEnabled(bool isEnabled);
    }

}
