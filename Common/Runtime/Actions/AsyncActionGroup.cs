using System;
using System.Buffers;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Common.Lists;
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
            
            int count = actions.Length;
            if (count == 1) {
                await actions[0].Apply(context, cancellationToken);
                return;
            }
            
            switch (mode) {
                case Mode.Sequence:
                    for (int i = 0; i < count; i++) {
                        if (cancellationToken.IsCancellationRequested) break;
                        if (actions[i] is {} action) await action.Apply(context, cancellationToken);
                    }
                    break;

                case Mode.Parallel:
                    var tasks = ArrayPool<UniTask>.Shared.Rent(count);

                    for (int i = 0; i < count; i++) {
                        tasks[i] = actions[i] is {} action ? action.Apply(context, cancellationToken) : UniTask.CompletedTask;
                    }

                    await UniTask.WhenAll(tasks);
                    tasks.ResetArrayElements();
                    
                    ArrayPool<UniTask>.Shared.Return(tasks);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
