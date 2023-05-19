﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Processors;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.View {
    
    [Serializable]
    public sealed class CharacterActionSetViewSmoothing : IAsyncAction, IDependency {

        [Min(0.001f)] public float viewSmoothFactor = 20f;

        private CharacterProcessorQuaternionSmoothing _smoothing;

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<CharacterAccess>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _smoothing = resolver
                .ResolveDependency<CharacterAccess>()
                .GetPipeline<ICharacterViewPipeline>()
                .GetProcessor<CharacterProcessorQuaternionSmoothing>();
        }

        public void Initialize() { }

        public void DeInitialize() { }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _smoothing.smoothFactor = viewSmoothFactor;
            return default;
        }
    }
    
}
