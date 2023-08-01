using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Jump {

    [Serializable]
    public sealed class CharacterActionJumpReaction : IAsyncAction, IDependency {

        public Case[] cases;

        [Serializable]
        public struct Case {
            public float minMagnitude;
            public float maxMagnitude;

            [SubclassSelector]
            [SerializeReference] public IAsyncAction action;
        }

        private ICharacterJumpPipeline _jump;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();

            for (int i = 0; i < cases.Length; i++) {
                if (cases[i].action is IDependency dep) dep.OnSetupDependencies(container);
            }
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _jump = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterJumpPipeline>();

            for (int i = 0; i < cases.Length; i++) {
                if (cases[i].action is IDependency dep) dep.OnResolveDependencies(resolver);
            }
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            float sqrMagnitude = _jump.LastJumpImpulse.sqrMagnitude;

            for (int i = 0; i < cases.Length; i++) {
                var c = cases[i];
                if (c.minMagnitude * c.minMagnitude <= sqrMagnitude && sqrMagnitude < c.maxMagnitude * c.maxMagnitude) {
                    return c.action.Apply(source, cancellationToken);
                }
            }

            return default;
        }
    }

}
