using System.Threading;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Steps {
    
    public sealed class CharacterStepsReactionPipeline : CharacterPipelineBase, ICharacterStepsReactionPipeline {

        [SerializeField] private CharacterAccess _characterAccess;

        [EmbeddedInspector]
        [SerializeField] private AsyncActionAsset _stepReaction;

        [RuntimeDependency(typeof(ICharacterAccess))]
        [FetchDependencies(nameof(_stepReaction))]
        [SerializeField] private DependencyResolver _dependencies;

        private ICharacterStepsPipeline _steps;
        private CancellationTokenSource _destroyCts;

        private void Awake() {
            _destroyCts = new CancellationTokenSource();
            _steps = _characterAccess.GetPipeline<ICharacterStepsPipeline>();

            _dependencies.SetValue<ICharacterAccess>(_characterAccess);
            _dependencies.Resolve(_stepReaction);
        }

        private void OnDestroy() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        public override void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _steps.OnStep -= OnStep;
                _steps.OnStep += OnStep;
                return;
            }

            _steps.OnStep -= OnStep;
        }

        private void OnStep(int foot, float distance, Vector3 point) {
            _stepReaction.TryApply(this, _destroyCts.Token);
        }
    }

}
