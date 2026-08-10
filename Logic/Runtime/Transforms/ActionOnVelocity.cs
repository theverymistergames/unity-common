using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.Tick;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Logic.Transforms {
    
    public sealed class ActionOnVelocity : MonoBehaviour, IActorComponent, IUpdate {
    
        [SerializeField] private Transform _target;
        [SerializeField] [Min(1)] private int _speedBufferSize = 3;
        [SerializeField] [Min(0f)] private float _speedThreshold = 1f;
        [SerializeReference] [SubclassSelector] private IActorAction _actionLower;
        [SerializeReference] [SubclassSelector] private IActorAction _actionHigher;
        [SerializeField] private bool _cancelLastAction = true;
        
        private CancellationTokenSource _enableCts;
        private readonly VelocityBuffer _velocityBuffer = new();
        private IActor _actor;
        private Vector3 _lastPosition;
        private bool _overThreshold;
        
        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            _velocityBuffer.SetBufferSize(_speedBufferSize);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
            _lastPosition = _target.position;
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var oldPos = _lastPosition;
            _lastPosition = _target.position;
            
            _velocityBuffer.WriteIntoBuffer(_lastPosition - oldPos, dt);

            float speed = GetSpeedFromBuffer();
            bool wasOverThreshold = _overThreshold;
            _overThreshold = speed > _speedThreshold;

            if (_overThreshold == wasOverThreshold) return;
            
            var lastAction = wasOverThreshold ? _actionHigher : _actionLower;
            var action = _overThreshold ? _actionHigher : _actionLower;
            
            if (_cancelLastAction && lastAction != null) {
                AsyncExt.RecreateCts(ref _enableCts);
            }
            
            action?.Apply(_actor, _enableCts.Token).Forget();
        }
        private float GetSpeedFromBuffer() {
            return _velocityBuffer.GetVelocity().magnitude;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void Reset() {
            _target = transform;
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo || !Application.isPlaying || _target == null) return;
            
            Handles.Label(_target.position, $"Vel speed = {GetSpeedFromBuffer()}");
        }

        private void OnValidate() {
            _velocityBuffer.SetBufferSize(_speedBufferSize);
        }
#endif
    }
    
}