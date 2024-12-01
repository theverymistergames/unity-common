using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Collisions.Triggers;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    public sealed class SceneTransactionTrigger : MonoBehaviour {

        [SerializeField] private Trigger _trigger;
        [SerializeField] [Min(0f)] private float _loadDelay;
        [SerializeField] private SceneTransaction _transaction;

        private void OnEnable() {
            _trigger.OnTriggered += OnTriggered;
        }

        private void OnDisable() {
            _trigger.OnTriggered -= OnTriggered;
        }

        private void OnTriggered(Collider go) {
            Apply(_transaction, _loadDelay, destroyCancellationToken).Forget();
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
