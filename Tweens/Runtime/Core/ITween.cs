using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.Tweens.Core {

    public interface ITween {
        void Initialize(MonoBehaviour owner);
        void DeInitialize();

        UniTask Play(CancellationToken token);

        void Wind();
        void Rewind();
        void Invert(bool isInverted);
    }

}
