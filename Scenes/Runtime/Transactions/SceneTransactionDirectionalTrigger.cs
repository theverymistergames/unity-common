using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Collisions.Triggers;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    public sealed class SceneTransactionDirectionalTrigger : MonoBehaviour {

        [SerializeField] private DirectionalTrigger _directionalTrigger;
        [SerializeField] [Min(0f)] private float _loadDelay;
        [SerializeField] private SceneTransaction _forwardSceneTransaction;
        [SerializeField] private SceneTransaction _backwardSceneTransaction;

        private void OnEnable() {
            _directionalTrigger.OnTriggeredForward += OnTriggeredForward;
            _directionalTrigger.OnTriggeredBackward += OnTriggeredBackward;
        }

        private void OnDisable() {
            _directionalTrigger.OnTriggeredForward -= OnTriggeredForward;
            _directionalTrigger.OnTriggeredBackward -= OnTriggeredBackward;
        }

        private void OnTriggeredForward(GameObject go) {
            Apply(_forwardSceneTransaction, _loadDelay, destroyCancellationToken).Forget();
        }

        private void OnTriggeredBackward(GameObject go) {
            Apply(_backwardSceneTransaction, _loadDelay, destroyCancellationToken).Forget();
        }

        private static async UniTaskVoid Apply(SceneTransaction transaction, float delay, CancellationToken token) {
            await UniTask
                .Delay(TimeSpan.FromSeconds(delay), cancellationToken: token)
                .SuppressCancellationThrow();

            if (token.IsCancellationRequested) return;

            await transaction.Apply(token);
        }
    }
    
}
