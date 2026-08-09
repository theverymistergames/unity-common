using System;
using MisterGames.Actors;
using MisterGames.Character.View;
using MisterGames.Common.Data;
using MisterGames.Common.Inputs;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Input.Actions;
using MisterGames.Interact.Interactives;
using MisterGames.Common.Tick;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Character.Interactives {
    
    [RequireComponent(typeof(Interactive))]
    public sealed class InteractiveRotation : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private Transform _target;
        [SerializeField] private InputActionRef _rotationInput;
        [SerializeField] private Vector2 _sensitivityMouse = Vector2.one;
        [SerializeField] private Vector2 _sensitivityGamepad = Vector2.one;
        [SerializeField] [Min(0f)] private float _smoothing = 10f;
        [SerializeField] [Min(0f)] private float _inputSmoothing = 1f;
        [SerializeField] [Range(0f, 180f)] private float _stopAngleThreshold = 2f;
        [SerializeField] private ViewClampProcessor _viewClamp;

        [Header("Exit")]
        [SerializeField] private InputActionRef _exitInput;
        [SerializeField] private Optional<InputDeviceType> _deviceType = new(InputDeviceType.Gamepad, true);
        
        private Interactive _interactive;
        private Vector2 _inputAccum;
        private Vector2 _targetOrientation;
        private Vector2 _smoothedOrientation;
        private float _smoothFactor;
        private bool _finishingFlag;
        
        private void Awake() {
            _interactive = GetComponent<Interactive>();

            var eulers = _target.eulerAngles.ToEulerAngles180();
            _targetOrientation = new Vector2(eulers.z, eulers.y);
            _smoothedOrientation = _targetOrientation;

            _viewClamp.SetViewOrientation(_smoothedOrientation);
            _viewClamp.ResetNextViewCenterOffset();
        }

        private void OnEnable() {
            _interactive.OnStartInteract += OnStartInteract;
            _interactive.OnStopInteract += OnStopInteract;
            
            ActualizeSubscriptions();
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
            
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;
            _exitInput.Get().performed -= OnExitInput;
            
            _inputAccum = Vector2.zero;
            _finishingFlag = false;
        }

        private void OnStartInteract(IInteractiveUser user) {
            ActualizeSubscriptions();
        }

        private void OnStopInteract(IInteractiveUser user) {
            ActualizeSubscriptions();
        }

        private void ActualizeSubscriptions() {
            if (_interactive.IsInteracting) {
                _finishingFlag = false;
                _exitInput.Get().performed -= OnExitInput;
                _exitInput.Get().performed += OnExitInput;
                PlayerLoopStage.Update.Subscribe(this);
                return;
            }
            
            _exitInput.Get().performed -= OnExitInput;
            _finishingFlag = true;
        }

        private void OnExitInput(InputAction.CallbackContext obj) {
            if (_deviceType.HasValue && _deviceType.Value != Services.Get<IDeviceService>().CurrentDevice || 
                Services.Get<IUiWindowService>().HasOpenedWindows()) return;
            
            _interactive.ForceStopInteractWithAllUsers();
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_finishingFlag) {
                var delta = _rotationInput.Get().ReadValue<Vector2>();
                var sens = Services.Get<IDeviceService>().CurrentDevice switch {
                    InputDeviceType.KeyboardMouse => _sensitivityMouse,
                    InputDeviceType.Gamepad => _sensitivityGamepad,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                _inputAccum += new Vector2(delta.y, delta.x) * sens;
            }
            
            float consume = _inputSmoothing * dt;
            _targetOrientation += consume * _inputAccum;
            _inputAccum *= Mathf.Max(1f - consume, 0f);
            
            _viewClamp.Process(_target.position, Quaternion.identity, ref _smoothedOrientation, ref _targetOrientation, dt);
            _smoothedOrientation = _smoothedOrientation.SmoothExpNonZero(_targetOrientation, _smoothing, dt);
            
            _target.rotation = Quaternion.Euler(0f, _smoothedOrientation.y, _smoothedOrientation.x);

            if (_finishingFlag && 
                _inputAccum == Vector2.zero && 
                Vector3.Angle(_smoothedOrientation, _targetOrientation) <= _stopAngleThreshold) 
            {
                _finishingFlag = false;
                PlayerLoopStage.Update.Unsubscribe(this);
            }
        }
    }
    
}