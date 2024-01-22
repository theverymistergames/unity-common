using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.Tweens {

    public class TweenRunner : MonoBehaviour {

        [SerializeField] private bool _playAtStart;
        [SerializeField] private TweenPlayer _tweenPlayer;

        public TweenPlayer TweenPlayer => _tweenPlayer;

        private CancellationTokenSource _enableCts;

        private void Awake() {
            Debug.Log($"TweenRunner.Awake: {name}");
        }

        private void OnEnable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = new CancellationTokenSource();
        }

        private void OnDisable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = null;

            _tweenPlayer.Stop();
        }

        private void OnDestroy() {
            _tweenPlayer.Stop();
        }

        private void Start() {
            if (_playAtStart) _tweenPlayer.Play(cancellationToken: _enableCts.Token).Forget();
        }
    }

}
