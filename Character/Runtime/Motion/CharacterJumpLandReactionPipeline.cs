using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class CharacterJumpLandReactionPipeline : MonoBehaviour, IActorComponent {
        
        [SerializeReference] [SubclassSelector] private IActorAction _jumpAction;
        [SerializeField] private LandingOption[] _landingOptions;

        [Serializable]
        private struct LandingOption {
            [Range(-1f, 0f)] public float relativeSpeed;
            [SerializeReference] [SubclassSelector]
            public IActorAction action;
        }

        private CancellationTokenSource _enableCts;
        private IActor _actor;
        private CharacterJumpPipeline _jumpPipeline;
        private CharacterLandingDetector _landingDetector;
        private CharacterMotionPipeline _motionPipeline;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
            _jumpPipeline = actor.GetComponent<CharacterJumpPipeline>();
            _landingDetector = actor.GetComponent<CharacterLandingDetector>();
            _motionPipeline = actor.GetComponent<CharacterMotionPipeline>();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);

            _jumpPipeline.OnJumpRequest += JumpPipelineRequest;
            _landingDetector.OnLanded += OnLanded;
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _jumpPipeline.OnJumpRequest -= JumpPipelineRequest;
            _landingDetector.OnLanded -= OnLanded;
        }

        private void JumpPipelineRequest() {
            _jumpAction?.Apply(_actor, _enableCts.Token).Forget();
        }

        private void OnLanded(Vector3 point, float relativeSpeed) {
            if (_motionPipeline.IsKinematic) return;
            
            for (int i = _landingOptions.Length - 1; i >= 0; i--)
            {
                ref var option = ref _landingOptions[i];
                if (relativeSpeed > option.relativeSpeed) continue;
                
                option.action?.Apply(_actor, _enableCts.Token).Forget();
                break;
            }
        }
    }

}
