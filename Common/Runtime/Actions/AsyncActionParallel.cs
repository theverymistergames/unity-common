using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Common.Actions {

    [Serializable]
    public sealed class AsyncActionParallel : IAsyncAction, IDependency {

        [SerializeReference] [SubclassSelector] public IAsyncAction[] actions;

        private UniTask[] _tasks;

        public void OnSetupDependencies(IDependencyContainer container) {
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i] is IDependency dep) dep.OnSetupDependencies(container);
            }
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i] is IDependency dep) dep.OnResolveDependencies(resolver);
            }
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            if (_tasks == null) {
                int actionsCount = actions.Length;
                _tasks = actionsCount > 0 ? new UniTask[actionsCount] : Array.Empty<UniTask>();
            }

            for (int i = 0; i < _tasks.Length; i++) {
                _tasks[i] = actions[i].Apply(source, cancellationToken);
            }

            return UniTask.WhenAll(_tasks);
        }
    }

}
