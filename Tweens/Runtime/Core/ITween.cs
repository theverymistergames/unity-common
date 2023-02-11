using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.Tweens.Core {

    public interface ITween {
        void Initialize(MonoBehaviour owner);
        void DeInitialize();

        UniTask Play(CancellationToken token);

        void Wind(bool reportProgress = true);
        void Rewind(bool reportProgress = true);
        void Invert(bool isInverted);
    }

}
