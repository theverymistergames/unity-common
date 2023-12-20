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

        private CancellationTokenSource _destroyCts;

        private void Awake() {
            _destroyCts = new CancellationTokenSource();
        }

        private void OnDestroy() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
        }

        private void OnEnable() {
            _trigger.OnTriggered += OnTriggered;
        }

        private void OnDisable() {
            _trigger.OnTriggered -= OnTriggered;
        }

        private void OnTriggered(GameObject go) {
            Apply(_transaction, _loadDelay, _destroyCts.Token).Forget();
        }

        private static async UniTaskVoid Apply(SceneTransaction transaction, float delay, CancellationToken token) {
            bool isCancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(delay), cancellationToken: token)
                .SuppressCancellationThrow();

            if (isCancelled) return;

            await transaction.Apply();
        }
    }
    
}
