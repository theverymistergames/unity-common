using System;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Clocks {
    
    public sealed class ClockBehaviour  : MonoBehaviour, IActorComponent, IUpdate {
        
        [Header("Arrows")]
        [SerializeField] private Transform _circleCenter;
        [SerializeField] private Transform _hourArrow;
        [SerializeField] private Transform _minuteArrow;
        [SerializeField] private Transform _secondArrow;
        [SerializeField] private Vector3 _centerNormal;

        [Header("Actions")]
        [SerializeField] [Min(0f)] private float _ignoreStartDelay = 0.1f;
        [SerializeReference] [SubclassSelector] private IActorAction _evenTickAction;  
        [SerializeReference] [SubclassSelector] private IActorAction _oddTickAction; 
        
        public event Action<int> OnTick = delegate { };
        
        private Vector3 _hourOffset;
        private Vector3 _minuteOffset;
        private Vector3 _secondOffset;
        private Quaternion _hourRotationOffset;
        private Quaternion _minuteRotationOffset;
        private Quaternion _secondRotationOffset;
        private bool _hasHourArrow;
        private bool _hasMinuteArrow;
        private bool _hasSecondArrow;
        private int _lastSecond;
        private float _startTime; 
        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            _circleCenter.GetPositionAndRotation(out var pos, out var rot);
            var invRot = Quaternion.Inverse(rot);

            _hasHourArrow = _hourArrow != null;
            _hasMinuteArrow = _minuteArrow != null;
            _hasSecondArrow = _secondArrow != null;

            if (_hasHourArrow) {
                _hourOffset = invRot * (_hourArrow.position - pos);
                _hourRotationOffset = invRot * _hourArrow.rotation;
            }

            if (_hasMinuteArrow) {
                _minuteOffset = invRot * (_minuteArrow.position - pos);
                _minuteRotationOffset = invRot * _minuteArrow.rotation;
            }

            if (_hasSecondArrow) {
                _secondOffset = invRot * (_secondArrow.position - pos);
                _secondRotationOffset = invRot * _secondArrow.rotation;   
            }
        }

        private void Start() {
            _startTime = Time.realtimeSinceStartup;
        }

        private void OnEnable() {
            _lastSecond = ClockSystem.Now.Second;
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            ApplyTick();
            UpdateArrows();
        }

        private void ApplyTick() {
            int second = ClockSystem.Now.Second;
            if (_lastSecond == second) return;
            
            _lastSecond = second;

            if (Time.realtimeSinceStartup < _startTime + _ignoreStartDelay) return;
            
            var action = second % 2 == 0 ? _evenTickAction : _oddTickAction;
            action?.Apply(_actor, destroyCancellationToken).Forget();
            
            OnTick.Invoke(second);
        }

        private void UpdateArrows() {
            var now = ClockSystem.Now;
            _circleCenter.GetPositionAndRotation(out var pos, out var rot);
            var normal = rot * _centerNormal;

            if (_hasHourArrow) {
                var angle = Quaternion.AngleAxis(now.Hour * 15f, normal) * rot;
                _hourArrow.SetPositionAndRotation(pos + angle * _hourOffset, angle * _hourRotationOffset);
            }
            
            if (_hasMinuteArrow) {
                var angle = Quaternion.AngleAxis(now.Minute * 6f, normal) * rot;
                _minuteArrow.SetPositionAndRotation(pos + angle * _minuteOffset, angle * _minuteRotationOffset);
            }
            
            if (_hasSecondArrow) {
                var angle = Quaternion.AngleAxis(now.Second * 6f, normal) * rot;
                _secondArrow.SetPositionAndRotation(pos + angle * _secondOffset, angle * _secondRotationOffset);
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;
            
            Handles.Label(_circleCenter.position + _circleCenter.rotation * _centerNormal * 0.05f, ClockSystem.Now.ToString("HH:mm:ss.fff"));
        }
#endif
    }
    
}