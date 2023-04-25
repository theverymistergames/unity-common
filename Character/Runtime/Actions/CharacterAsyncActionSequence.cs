using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {
    
    [Serializable]
    public sealed class CharacterAsyncActionSequence : ICharacterAsyncAction, ICharacterAccessInitializable {

        [SerializeReference] [SubclassSelector] public ICharacterAsyncAction[] actions;

        public void Initialize(ICharacterAccess characterAccess) {
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i] is ICharacterAccessInitializable action) action.Initialize(characterAccess);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i] is ICharacterAccessInitializable action) action.DeInitialize();
            }
        }

        public async UniTask ApplyAsync(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            for (int i = 0; i < actions.Length; i++) {
                await actions[i].ApplyAsync(source, characterAccess, cancellationToken);
            }
        }
    }
    
}
