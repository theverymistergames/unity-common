using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Tweens.Core {

    public class TweenRunner : MonoBehaviour {

        [SerializeField] private bool _playAtStart;

        [SerializeReference] [SubclassSelector]
        private ITween _tween;

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _pauseCts;

        private void Awake() {
            _tween.Initialize(this);

            _destroyCts = new CancellationTokenSource();
        }

        private void OnDestroy() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();

            _tween.DeInitialize();
        }

        private void Start() {
            if (_playAtStart) Play();
        }

        public void Play() {
            _pauseCts?.Dispose();
            _pauseCts = new CancellationTokenSource();

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_pauseCts.Token, _destroyCts.Token);

            _tween.Play(linkedCts.Token).Forget();
        }

        public void Pause() {
            _pauseCts?.Cancel();
        }

        public void Wind() {
            Pause();
            _tween.Wind();
        }

        public void Rewind() {
            Pause();
            _tween.Rewind();
        }

        public void Invert(bool isInverted) {
            _tween.Invert(isInverted);
        }
    }

}
