using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Startup {

    public sealed class CharacterStartup : MonoBehaviour, IActorComponent {

        [EmbeddedInspector]
        [SerializeField] private ActorAction[] _startupActions;

        private IActor _actor;
        private CancellationTokenSource _enableCts;

        public void OnAwakeActor(IActor actor) {
            _actor = actor;
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
        }

        private void Start() {
            Apply(_enableCts.Token).Forget();
        }

        private async UniTask Apply(CancellationToken token) {
            for (int i = 0; i < _startupActions.Length; i++) {
                await _startupActions[i].Apply(_actor, token);
            }
        }
    }

}
