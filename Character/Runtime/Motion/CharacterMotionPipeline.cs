using MisterGames.Actors;
using MisterGames.Character.Input;
using MisterGames.Character.Processors;
using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class CharacterMotionPipeline : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;
        
        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2[] _inputProcessors = {
            new CharacterProcessorBackSideSpeedCorrection { speedCorrectionBack = 0.6f, speedCorrectionSide = 0.8f },
            new CharacterProcessorVector2Multiplier { multiplier = 5f },
            new CharacterProcessorVector2Smoothing { smoothFactor = 20f },
        };
        
        [SerializeField] private CharacterMassProcessor _massProcessor = new() {
            gravityForce = 15f,
            airInertialFactor = 10f,
            groundInertialFactor = 20f,
            inputInfluenceFactor = 0.5f,
        };

        private readonly CharacterForwardDirectionProcessor _forwardProcessor = new();
        
        public Vector2 MotionInput { get; private set; }
        public Vector3 Velocity => _massProcessor.CurrentVelocity;

        private IActor _actor;
        private ITimeSource _timeSource;
        private ITransformAdapter _bodyAdapter;
        private CharacterInputPipeline _input;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
            _input = actor.GetComponent<CharacterInputPipeline>();
            _bodyAdapter = actor.GetComponent<CharacterBodyAdapter>();
            _timeSource = TimeSources.Get(_playerLoopStage);
        }
        
        private void OnEnable() {
            _massProcessor.Initialize(_actor);
            _forwardProcessor.Initialize(_actor);
            
            _input.OnMotionVectorChanged += HandleMotionInput;
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            _massProcessor.DeInitialize(_actor);
            _forwardProcessor.DeInitialize(_actor);
            
            _input.OnMotionVectorChanged -= HandleMotionInput;
            _timeSource.Unsubscribe(this);
        }

        public T GetProcessor<T>() {
            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is T ip) return ip;
            }

            if (_massProcessor is T t0) return t0;
            
            if (_forwardProcessor is T t1) return t1;

            return default;
        }

        private void HandleMotionInput(Vector2 input) {
            MotionInput = input;
        }

        public void OnUpdate(float dt) {
            var input = MotionInput;
            
            for (int i = 0; i < _inputProcessors.Length; i++) {
                input = _inputProcessors[i].Process(input, dt);
            }

            var motion = _forwardProcessor.Process(input, dt);
            motion = _massProcessor.Process(motion, dt);

            _bodyAdapter.Move(motion * dt);
        }
    }

}
