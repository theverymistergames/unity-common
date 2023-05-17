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

        public void OnAddDependencies(IDependencyResolver resolver) {
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i] is IDependency dep) dep.OnAddDependencies(resolver);
            }
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i] is IDependency dep) dep.OnResolveDependencies(resolver);
            }
        }

        public void Initialize() {
            _tasks = new UniTask[actions.Length];

            for (int i = 0; i < actions.Length; i++) {
                actions[i].Initialize();
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].DeInitialize();
            }
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            for (int i = 0; i < _tasks.Length; i++) {
                _tasks[i] = actions[i].Apply(source, cancellationToken);
            }

            return UniTask.WhenAll(_tasks);
        }
    }

}
