using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Actions {

    [Serializable]
    public class AsyncActionGroup<T, A> : IAsyncAction<T> where A : IAsyncAction<T> {

        public Mode mode;
        [SerializeReference] [SubclassSelector] public A[] actions;

        public enum Mode {
            Sequence,
            Parallel,
        }

        public async UniTask Apply(T context, CancellationToken cancellationToken = default) {
            if (actions is not { Length: > 0 }) return;

            switch (mode) {
                case Mode.Sequence:
                    for (int i = 0; i < actions.Length; i++) {
                        await actions[i].Apply(context, cancellationToken);
                    }
                    break;

                case Mode.Parallel:
                    var tasks = new UniTask[actions.Length];
                    for (int i = 0; i < actions.Length; i++) {
                        tasks[i] = actions[i].Apply(context, cancellationToken);
                    }
                    await UniTask.WhenAll(tasks);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
