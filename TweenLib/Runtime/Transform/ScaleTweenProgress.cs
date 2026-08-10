using System;
using System.Threading;
using MisterGames.Actors;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    public sealed class ScaleTweenProgress : MonoBehaviour, IActorComponent, IUpdate {
        
        [Header("Scale")]
        [SerializeField] private Transform _target;
        [SerializeField] [Min(0f)] private float _scaleMin = 0f;
        [SerializeField] [Min(0f)] private float _scaleMax = 1f;
        [SerializeField] private Mode _mode = Mode.MinAxis;

        [Header("Tweens")]
        [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeReference] [SubclassSelector] private IProgressModulator _modulator;
        [SerializeReference] [SubclassSelector] private ITweenProgressAction _action;
        [SerializeField] private TweenEvent[] _events;
        
        private enum Mode {
            MinAxis,
            MaxAxis,
        }

        private CancellationTokenSource _enableCts;
        private IActor _actor;
        private float _progress;
        
        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            PlayerLoopStage.LateUpdate.Subscribe(this);

            _progress = GetProgress();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            float oldProgress = _progress;
            _progress = GetProgress();
            
            _action.OnProgressUpdate(_progress);
            _events.NotifyTweenEvents(_actor, _progress, oldProgress, _enableCts.Token);
        }

        private float GetProgress() {
            float s = GetScaleValue(_target.localScale);
            float t = _scaleMin < _scaleMax
                ? (s - _scaleMin) / (_scaleMax - _scaleMin) 
                : s < _scaleMin ? 0f : 1f;

            float p = t;
            p = _curve?.Evaluate(p) ?? p;
            return _modulator?.Modulate(p) ?? p;
        }
        
        private float GetScaleValue(Vector3 scale) {
            return _mode switch {
                Mode.MinAxis => scale.Abs().MinAxis(),
                Mode.MaxAxis => scale.Abs().MaxAxis(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_scaleMax < _scaleMin) _scaleMax = _scaleMin;
        }
#endif
    }
    
}