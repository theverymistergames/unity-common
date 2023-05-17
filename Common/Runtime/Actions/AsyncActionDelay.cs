using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Common.Actions {

    [Serializable]
    public sealed class AsyncActionDelay : IAsyncAction {

        [Min(0f)] public float duration;
        [Min(0f)] public float randomAdditionMax;

        public void Initialize() { }

        public void DeInitialize() { }

        public async UniTask Apply(object source, CancellationToken cancellationToken = default) {
            float d = Mathf.Max(0f, duration + Random.Range(-randomAdditionMax, randomAdditionMax));
            await UniTask
                .Delay(TimeSpan.FromSeconds(d), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
        }
    }

}
