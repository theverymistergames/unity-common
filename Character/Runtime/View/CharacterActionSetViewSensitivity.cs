using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Processors;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.View {
    
    [Serializable]
    public sealed class CharacterActionSetViewSensitivity : IAsyncAction, IDependency {

        [Min(0.001f)] public float sensitivityHorizontal = 0.15f;
        [Min(0.001f)] public float sensitivityVertical = 0.15f;

        private CharacterProcessorVector2Sensitivity _sensitivity;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _sensitivity = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterViewPipeline>()
                .GetProcessor<CharacterProcessorVector2Sensitivity>();
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _sensitivity.sensitivity = new Vector2(sensitivityHorizontal, sensitivityVertical);
            return default;
        }
    }
    
}
