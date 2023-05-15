using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Character.Animations {

    [Serializable]
    public sealed class CharacterActionDelay : ICharacterAction {

        [Min(0f)] public float duration;
        [Min(0f)] public float randomAdditionMax;

        public async UniTask Apply(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            float d = Mathf.Max(0f, duration + Random.Range(-randomAdditionMax, randomAdditionMax));
            await UniTask
                .Delay(TimeSpan.FromSeconds(d), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
        }
    }

}
