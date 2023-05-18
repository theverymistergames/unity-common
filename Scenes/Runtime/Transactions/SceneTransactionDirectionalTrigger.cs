using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Collisions.Triggers;
using MisterGames.Common.Actions;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    public sealed class SceneTransactionDirectionalTrigger : MonoBehaviour {

        [SerializeField] private DirectionalTrigger _directionalTrigger;
        [SerializeField] [Min(0f)] private float _loadDelay;
        [SerializeField] private SceneTransaction _forwardSceneTransaction;
        [SerializeField] private SceneTransaction _backwardSceneTransaction;

        private CancellationTokenSource _destroyCts;

        private void Awake() {
            _destroyCts = new CancellationTokenSource();
        }

        private void OnDestroy() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
        }

        private void OnEnable() {
            _directionalTrigger.OnTriggeredForward += OnTriggeredForward;
            _directionalTrigger.OnTriggeredBackward += OnTriggeredBackward;
        }

        private void OnDisable() {
            _directionalTrigger.OnTriggeredForward -= OnTriggeredForward;
            _directionalTrigger.OnTriggeredBackward -= OnTriggeredBackward;
        }

        private void OnTriggeredForward(GameObject go) {
            ApplyDelayed(_forwardSceneTransaction, _loadDelay, _destroyCts.Token).Forget();
        }

        private void OnTriggeredBackward(GameObject go) {
            ApplyDelayed(_backwardSceneTransaction, _loadDelay, _destroyCts.Token).Forget();
        }

        private async UniTaskVoid ApplyDelayed(IAsyncAction action, float delay, CancellationToken token) {
            bool isCancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(delay), cancellationToken: token)
                .SuppressCancellationThrow();

            if (isCancelled) return;

            await action.Apply(this, token);
        }
    }
    
}
