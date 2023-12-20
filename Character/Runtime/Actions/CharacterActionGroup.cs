using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionGroup : ICharacterAction {

        public Mode mode;
        [SerializeReference] [SubclassSelector] public ICharacterAction[] actions;

        public enum Mode {
            Sequence,
            Parallel,
        }

        public async UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default) {
            if (actions is not { Length: > 0 }) return;

            switch (mode) {
                case Mode.Sequence:
                    for (int i = 0; i < actions.Length; i++) {
                        await actions[i].Apply(characterAccess, source, cancellationToken);
                    }
                    break;

                case Mode.Parallel:
                    var tasks = new UniTask[actions.Length];
                    for (int i = 0; i < actions.Length; i++) {
                        tasks[i] = actions[i].Apply(characterAccess, source, cancellationToken);
                    }
                    await UniTask.WhenAll(tasks);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
