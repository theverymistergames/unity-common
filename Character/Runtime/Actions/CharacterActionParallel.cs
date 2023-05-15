using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionParallel : ICharacterAction, ICharacterAccessInitializable {

        [SerializeReference] [SubclassSelector] public ICharacterAction actionA;
        [SerializeReference] [SubclassSelector] public ICharacterAction actionB;

        public void Initialize(ICharacterAccess characterAccess) {
            if (actionA is ICharacterAccessInitializable a) a.Initialize(characterAccess);
            if (actionB is ICharacterAccessInitializable b) b.Initialize(characterAccess);
        }

        public void DeInitialize() {
            if (actionA is ICharacterAccessInitializable a) a.DeInitialize();
            if (actionB is ICharacterAccessInitializable b) b.DeInitialize();
        }

        public UniTask Apply(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            return UniTask.WhenAll(
                actionA.Apply(source, characterAccess, cancellationToken),
                actionB.Apply(source, characterAccess, cancellationToken)
            );
        }
    }

}
