using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Logic.Transforms {
    
    public sealed class ActionOnRotation : MonoBehaviour, IActorComponent, IUpdate {
    
        [SerializeField] private Transform _target;
        [SerializeField] private bool _useLocal = true;
        [SerializeField] [Range(0f, 180f)] private float _minAngleChange = 3f;
        [SerializeField] [Min(0f)] private float _minAngularSpeed = 1f;
        [SerializeField] [Min(0f)] private float _minCooldown = 1f;
        [SerializeField] [Min(1)] private int _speedBufferSize = 3;
        [SerializeReference] [SubclassSelector] private IActorAction _action;
        
        private CancellationTokenSource _enableCts;
        
        private (float diff, float dt)[] _speedBuffer;
        private IActor _actor;
        private Quaternion _rotation;
        private Quaternion _lastRotation;
        private float _lastActionTime;
        private int _speedBufferPointer;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            _speedBuffer = new (float diff, float dt)[_speedBufferSize];
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            PlayerLoopStage.LateUpdate.Subscribe(this);

            _rotation = GetRotation();
            _lastRotation = _rotation;
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var rot = GetRotation();
            var lastRot = _lastRotation;

            _lastRotation = rot;
            
            float angle = Quaternion.Angle(_rotation, rot);
            float diff = Quaternion.Angle(lastRot, rot);
            
            WriteIntoSpeedBuffer(diff, dt);
            float angularSpeed = GetSpeedFromBuffer();

            if (angle < _minAngleChange) return;
            
            _rotation = rot;
            
            if (angularSpeed < _minAngularSpeed || Time.time < _lastActionTime + _minCooldown) return;

            _lastActionTime = Time.time;
            _action?.Apply(_actor, _enableCts.Token).Forget();
        }

        private void WriteIntoSpeedBuffer(float diff, float dt) {
            _speedBuffer[_speedBufferPointer++ % _speedBufferSize] = (diff, dt);
        }

        private float GetSpeedFromBuffer() {
            int count = Mathf.Min(_speedBufferPointer, _speedBufferSize);
            float diff = 0f;
            float time = 0f;
            
            for (int i = 0; i < count; i++) {
                var data = _speedBuffer[i];
                diff += data.diff;
                time += data.dt;
            }
            
            return time > 0f ? diff / time : 0f;
        }
        
        private Quaternion GetRotation() {
            return _useLocal ? _target.localRotation : _target.rotation;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void Reset() {
            _target = transform;
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo || !Application.isPlaying || _target == null) return;
            
            Handles.Label(_target.position, $"Rot speed = {GetSpeedFromBuffer()}");
        }
#endif
    }
    
}