using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Tweens {

    public sealed class TweenRunner : MonoBehaviour, IActorComponent {

        [SerializeField] private string _name;
        [SerializeField] private bool _playAtStart;
        [Space(10f)]
        [SerializeField] private TweenPlayer<IActor, IActorTween> _tweenPlayer;
        [SerializeField] private TweenEvent[] _events;
        
        public TweenPlayer<IActor, IActorTween> TweenPlayer => _tweenPlayer;

        private IActor _actor;
        private CancellationTokenSource _enableCts;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
            _tweenPlayer.Context = actor;
        }

        private void OnEnable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = new CancellationTokenSource();

            _tweenPlayer.OnProgressUpdate += OnProgressUpdate;
            
            if (!_playAtStart) return;

            _tweenPlayer.Play(cancellationToken: _enableCts.Token).Forget();
        }

        private void OnDisable() {
            _tweenPlayer.OnProgressUpdate -= OnProgressUpdate;
            
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = null;
        }

        private void OnProgressUpdate(float progress, float oldProgress) {
            for (int i = 0; i < _events.Length; i++) {
                ref var e = ref _events[i];

                if (progress > oldProgress && oldProgress <= e.progress && e.progress <= progress || 
                    progress < oldProgress && progress <= e.progress && e.progress <= oldProgress
                ) {
                    e.action?.Apply(_actor, _enableCts.Token).Forget();
                }
            }
        }
    }

}
