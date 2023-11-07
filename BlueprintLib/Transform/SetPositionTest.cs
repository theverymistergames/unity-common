using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    public class SetPositionTest : MonoBehaviour {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 position0;
        [SerializeField] private Vector3 position1;
        [SerializeField] private float _wait;

        private CancellationTokenSource _destroyCts;
        private int _nextPositionIndex;
        private Vector3 _initialPosition;

        private void Awake() {
            _destroyCts?.Cancel();
            _destroyCts?.Dispose();
            _destroyCts = new CancellationTokenSource();

            _initialPosition = _transform.position;
        }

        private void OnDestroy() {
            _destroyCts?.Cancel();
            _destroyCts?.Dispose();
        }

        private async void Start() {
            await StartSequence(_destroyCts.Token);
        }

        private async UniTask StartSequence(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                bool isCancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(_wait), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();

                if (isCancelled) break;

                ChangePosition();
            }
        }

        private void ChangePosition() {
            var nextPosition = _initialPosition;

            switch (_nextPositionIndex) {
                case 0:
                    nextPosition += position0;
                    _nextPositionIndex = 1;
                    break;
                case 1:
                    nextPosition += position1;
                    _nextPositionIndex = 0;
                    break;
            }

            _transform.position = nextPosition;
        }
    }

}
