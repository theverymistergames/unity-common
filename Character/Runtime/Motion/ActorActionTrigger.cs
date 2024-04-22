using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Collisions.Triggers;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class ActorActionTrigger : MonoBehaviour {

        [SerializeField] private Trigger _enterTrigger;
        [SerializeField] private Trigger _exitTrigger;

        [SerializeField] private ActorAction _enterAction;
        [SerializeField] private ActorAction _exitAction;

        private CancellationTokenSource _enableCts;

        private void OnEnable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = new CancellationTokenSource();

            _enterTrigger.OnTriggered -= OnEnterTriggered;
            _enterTrigger.OnTriggered += OnEnterTriggered;

            _exitTrigger.OnTriggered -= OnExitTriggered;
            _exitTrigger.OnTriggered += OnExitTriggered;
        }

        private void OnDisable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = null;

            _enterTrigger.OnTriggered -= OnEnterTriggered;
            _exitTrigger.OnTriggered -= OnExitTriggered;
        }

        private void OnEnterTriggered(Collider obj) {
            if (!obj.TryGetComponent(out IActor actor)) return;

            _enterAction.Apply(actor, _enableCts.Token).Forget();
        }

        private void OnExitTriggered(Collider obj) {
            if (!obj.TryGetComponent(out IActor actor)) return;

            _exitAction.Apply(actor, _enableCts.Token).Forget();
        }
    }

}
