using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterAsyncActionParallel : ICharacterAsyncAction, ICharacterAccessInitializable {

        [SerializeReference] [SubclassSelector] public ICharacterAsyncAction actionA;
        [SerializeReference] [SubclassSelector] public ICharacterAsyncAction actionB;

        public void Initialize(ICharacterAccess characterAccess) {
            if (actionA is ICharacterAccessInitializable a) a.Initialize(characterAccess);
            if (actionB is ICharacterAccessInitializable b) b.Initialize(characterAccess);
        }

        public void DeInitialize() {
            if (actionA is ICharacterAccessInitializable a) a.DeInitialize();
            if (actionB is ICharacterAccessInitializable b) b.DeInitialize();
        }

        public UniTask ApplyAsync(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            return UniTask.WhenAll(
                actionA.ApplyAsync(source, characterAccess, cancellationToken),
                actionB.ApplyAsync(source, characterAccess, cancellationToken)
            );
        }
    }

}
