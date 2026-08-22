using System;
using MisterGames.Actors;
using MisterGames.Character.Input;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterMotionRunPipeline : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private Mode _keyboardMode = Mode.WhilePressed;
        [SerializeField] private Mode _gamepadMode = Mode.Toggle;

        private enum Mode {
            Toggle,
            WhilePressed,
        }

        public event Action OnRunStateChanged = delegate { };
        public bool IsRunActive { get; private set; }
        
        private IDeviceService _deviceService;
        private CharacterMotionPipeline _motion;
        private CharacterInputPipeline _input;

        private float _lastTimeHasNoMotionInput;
        private float _lastTimeRunPressed;
        
        void IActorComponent.OnAwake(IActor actor) {
            _motion = actor.GetComponent<CharacterMotionPipeline>();
            _input = actor.GetComponent<CharacterInputPipeline>();
            _deviceService = Services.Get<IDeviceService>();
        }

        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
            
            UpdateRunState(force: true);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);

            IsRunActive = false;
        }

        void IUpdate.OnUpdate(float dt) {
            UpdateRunState(force: false);
        }

        private void UpdateRunState(bool force) {
            var mode = GetCurrentMode();
            bool hasInput = _motion.Input != default;
            bool hasRunPressed = _input.IsRunPressed;
            
            if (!hasInput) {
                _lastTimeHasNoMotionInput = TimeSources.scaledTime;
            }

            if (hasRunPressed && hasInput) {
                _lastTimeRunPressed = TimeSources.scaledTime;
            }

            bool wasRunActive = IsRunActive;
            IsRunActive = mode switch {
                Mode.Toggle => hasInput && (hasRunPressed || _lastTimeRunPressed >= _lastTimeHasNoMotionInput),
                Mode.WhilePressed => hasInput && hasRunPressed,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (force || wasRunActive != IsRunActive) OnRunStateChanged.Invoke(); 
        }

        private Mode GetCurrentMode() {
            return _deviceService.CurrentDevice switch {
                InputDeviceType.KeyboardMouse => _keyboardMode,
                InputDeviceType.Gamepad => _gamepadMode,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
}