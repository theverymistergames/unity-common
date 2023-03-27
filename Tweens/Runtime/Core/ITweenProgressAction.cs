using UnityEngine;

namespace MisterGames.Tweens.Core {

    public interface ITweenProgressAction {
        void Initialize(MonoBehaviour owner);
        void DeInitialize();

        void Start();
        void Finish();

        void OnProgressUpdate(float progress);
    }

}
