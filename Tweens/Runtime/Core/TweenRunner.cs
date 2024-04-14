using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.Tweens {

    public sealed class TweenRunner : MonoBehaviour {

        [SerializeField] private bool _playAtStart;
        [SerializeField] private TweenPlayer _tweenPlayer;

        public TweenPlayer TweenPlayer => _tweenPlayer;

        private CancellationTokenSource _enableCts;

        private void OnEnable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = new CancellationTokenSource();

            if (_playAtStart) {
                _tweenPlayer.Play(cancellationToken: _enableCts.Token).Forget();
            }
        }

        private void OnDisable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = null;
        }
    }

}
