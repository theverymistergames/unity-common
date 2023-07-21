﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Common.Actions {
    
    [Serializable]
    public sealed class AsyncActionSequence : IAsyncAction, IDependency {

        [SerializeReference] [SubclassSelector] public IAsyncAction[] actions;

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

        public async UniTask Apply(object source, CancellationToken cancellationToken = default) {
            for (int i = 0; i < actions.Length; i++) {
                await actions[i].Apply(source, cancellationToken);
            }
        }
    }
    
}