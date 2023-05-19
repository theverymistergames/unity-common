using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Jump {

    [Serializable]
    public sealed class CharacterActionSetJumpForceMultiplier : IAsyncAction, IDependency {

        [Min(0f)] public float jumpForceMultiplier = 1f;

        private ICharacterJumpPipeline _jump;

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<CharacterAccess>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _jump = resolver
                .ResolveDependency<CharacterAccess>()
                .GetPipeline<ICharacterJumpPipeline>();
        }

        public void Initialize() { }

        public void DeInitialize() { }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _jump.ForceMultiplier = jumpForceMultiplier;
            return default;
        }
    }

}
