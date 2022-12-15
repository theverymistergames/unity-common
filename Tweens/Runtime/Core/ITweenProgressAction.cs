using UnityEngine;

namespace MisterGames.Tweens.Core {

    public interface ITweenProgressAction {
        void Initialize(MonoBehaviour owner);
        void DeInitialize();

        void OnProgressUpdate(float progress);
    }

}
