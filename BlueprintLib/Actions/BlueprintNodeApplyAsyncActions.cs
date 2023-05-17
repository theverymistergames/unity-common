﻿using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Apply Async Actions", Category = "Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeApplyAsyncActions : BlueprintNode, IBlueprintEnter, IDependencyResolver {

        [SerializeField] private AsyncActionAsset[] _applyActions;
        [SerializeField] private AsyncActionAsset[] _releaseActions;

        private UniTask[] _applyTasks;
        private UniTask[] _releaseTasks;
        private CancellationTokenSource _terminateCts;

        private readonly List<Type> _dependencies = new List<Type>();
        private int _dependencyPortIterator;

        public override Port[] CreatePorts() {
            var ports = new List<Port> {
                Port.Enter("Apply"),
                Port.Enter("Release"),
                Port.Exit("On Applied"),
                Port.Exit("On Released"),
            };

            _dependencies.Clear();

            if (_applyActions == null || _releaseActions == null) return ports.ToArray();

            for (int i = 0; i < _applyActions.Length; i++) {
                if (_applyActions[i] is IDependency dep) dep.OnAddDependencies(this);
            }

            for (int i = 0; i < _releaseActions.Length; i++) {
                if (_releaseActions[i] is IDependency dep) dep.OnAddDependencies(this);
            }

            for (int i = 0; i < _dependencies.Count; i++) {
                ports.Add(Port.DynamicInput(type: _dependencies[i]));
            }

            return ports.ToArray();
        }

        public override void OnInitialize(IBlueprintHost host) {
            _terminateCts?.Cancel();
            _terminateCts?.Dispose();
            _terminateCts = new CancellationTokenSource();

            _applyTasks = new UniTask[_applyActions.Length];
            _releaseTasks = new UniTask[_releaseActions.Length];
        }

        public override void OnDeInitialize() {
            _terminateCts?.Cancel();
            _terminateCts?.Dispose();
            _terminateCts = null;
        }

        public void AddDependency<T>(object source) {
            _dependencies.Add(typeof(T));
        }

        public T ResolveDependency<T>() {
            return Ports[_dependencyPortIterator++].Get<T>();
        }

        public async void OnEnterPort(int port) {
            switch (port) {
                case 0: {
                    _dependencyPortIterator = 4;

                    for (int i = 0; i < _applyActions.Length; i++) {
                        var action = _applyActions[i];
                        if (action is IDependency dep) dep.OnResolveDependencies(this);
                        action.Initialize();

                        _applyTasks[i] = action.Apply(this, _terminateCts.Token);
                    }

                    await UniTask.WhenAll(_applyTasks);

                    Ports[2].Call();

                    for (int i = 0; i < _applyActions.Length; i++) {
                        _applyActions[i].DeInitialize();
                    }

                    break;
                }

                case 1: {
                    _dependencyPortIterator = 4;

                    for (int i = 0; i < _releaseActions.Length; i++) {
                        var action = _releaseActions[i];
                        if (action is IDependency dep) dep.OnResolveDependencies(this);
                        action.Initialize();

                        _releaseTasks[i] = action.Apply(this, _terminateCts.Token);
                    }

                    await UniTask.WhenAll(_releaseTasks);

                    Ports[3].Call();

                    for (int i = 0; i < _releaseActions.Length; i++) {
                        _releaseActions[i].DeInitialize();
                    }

                    break;
                }
            }
        }
    }

}
