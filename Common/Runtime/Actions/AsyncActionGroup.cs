using System;
using System.Buffers;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Actions {

    [SubclassSelectorIgnore]
    [Serializable]
    public class AsyncActionGroup<TAction, TContext> : IAsyncAction<TContext>
        where TAction : IAsyncAction<TContext>
    {
        public Mode mode;
        [SerializeReference] [SubclassSelector] public TAction[] actions;

        public enum Mode {
            Sequence,
            Parallel,
        }

        public async UniTask Apply(TContext context, CancellationToken cancellationToken = default) {
            if (actions is not { Length: > 0 }) return;

            switch (mode) {
                case Mode.Sequence:
                    for (int i = 0; i < actions.Length; i++) {
                        await actions[i].Apply(context, cancellationToken);
                    }
                    break;

                case Mode.Parallel:
                    var tasks = ArrayPool<UniTask>.Shared.Rent(actions.Length);

                    for (int i = 0; i < actions.Length; i++) {
                        tasks[i] = actions[i].Apply(context, cancellationToken);
                    }
                    await UniTask.WhenAll(tasks);

                    ArrayPool<UniTask>.Shared.Return(tasks);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
