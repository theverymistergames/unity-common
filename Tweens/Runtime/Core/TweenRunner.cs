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
            AsyncExt.RecreateCts(ref _enableCts);

            _tweenPlayer.OnProgressUpdate += OnProgressUpdate;
            
            if (!_playAtStart) return;

            _tweenPlayer.Play(cancellationToken: _enableCts.Token).Forget();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _tweenPlayer.OnProgressUpdate -= OnProgressUpdate;
        }

        private void OnProgressUpdate(float progress, float oldProgress) {
            _events.NotifyTweenEvents(_actor, progress, oldProgress, _enableCts.Token);
        }
    }

}
