using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Collisions.Triggers;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class CharacterActionTrigger : MonoBehaviour {

        [SerializeField] private Trigger _enterTrigger;
        [SerializeField] private Trigger _exitTrigger;

        [SerializeField] private CharacterActionAsset _enterAction;
        [SerializeField] private CharacterActionAsset _exitAction;

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

        private void OnEnterTriggered(GameObject obj) {
            if (obj.GetComponent<CharacterAccess>() is not {} characterAccess) return;

            _enterAction.Apply(characterAccess, _enableCts.Token).Forget();
        }

        private void OnExitTriggered(GameObject obj) {
            if (obj.GetComponent<CharacterAccess>() is not {} characterAccess) return;

            _exitAction.Apply(characterAccess, _enableCts.Token).Forget();
        }
    }

}
