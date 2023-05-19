using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Height {

    [Serializable]
    public sealed class CharacterActionChangeColliderHeight : IAsyncAction, IDependency {

        public Optional<float> sourceHeight;
        public Optional<float> targetHeight;
        public Optional<float> targetRadius;

        [Min(0f)] public float metersPerSecond;

        private ICharacterHeightPipeline _height;

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<CharacterAccess>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _height = resolver
                .ResolveDependency<CharacterAccess>()
                .GetPipeline<ICharacterHeightPipeline>();
        }

        public void Initialize() { }

        public void DeInitialize() { }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            if (targetRadius.HasValue) _height.Radius = targetRadius.Value;

            float currentHeight = _height.Height;

            float fromHeight = sourceHeight.GetOrDefault(currentHeight);
            float toHeight = targetHeight.GetOrDefault(currentHeight);

            float duration = metersPerSecond <= 0f ? 0f : Mathf.Abs(toHeight - fromHeight) / metersPerSecond;

            return _height.ApplyHeightChange(fromHeight, toHeight, duration, cancellationToken);
        }
    }

}
