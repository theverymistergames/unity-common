using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Common.Async;
using UnityEngine;

namespace MisterGames.Tweens {

    public sealed class TweenRunner : MonoBehaviour, IActorComponent {

#if UNITY_EDITOR
        [SerializeField] private string _name;
#endif
        
        [SerializeField] private bool _playAtStart;
        [Space(10f)]
        [SerializeField] private TweenPlayer<IActor, IActorTween> _tweenPlayer = new();
        [SerializeField] private TweenEvent[] _events = Array.Empty<TweenEvent>();
        
        public TweenPlayer<IActor, IActorTween> TweenPlayer => _tweenPlayer;

        private IActor _actor;
        private CancellationTokenSource _enableCts;
        private bool _enabled;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
            _tweenPlayer.Context = actor;
        }

        private void Awake() {
            _tweenPlayer.OnProgressUpdate += OnProgressUpdate;
        }

        private void OnDestroy() {
            _tweenPlayer.OnProgressUpdate -= OnProgressUpdate;
        }

        private void OnEnable() {
            _enabled = true;
            
            if (!_playAtStart) return;

            AsyncExt.RecreateCts(ref _enableCts);
            _tweenPlayer.Play(cancellationToken: _enableCts.Token).Forget();
        }

        private void OnDisable() {
            _enabled = false;
            
            AsyncExt.DisposeCts(ref _enableCts);
        }

        private void OnProgressUpdate(float progress, float oldProgress) {
            if (!_enabled) return;
            
            if (_enableCts == null) AsyncExt.RecreateCts(ref _enableCts);
            _events.NotifyTweenEvents(_actor, progress, oldProgress, _enableCts.Token);
        }
    }

}
