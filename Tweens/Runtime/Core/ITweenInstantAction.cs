using UnityEngine;

namespace MisterGames.Tweens.Core {

    public interface ITweenInstantAction {
        void Initialize(MonoBehaviour owner);
        void DeInitialize();

        void InvokeAction();
    }

}
