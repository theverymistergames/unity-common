using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Startup {

    public sealed class CharacterStartup : MonoBehaviour {

        [SerializeField] private CharacterAccess _characterAccess;

        [EmbeddedInspector]
        [SerializeField] private CharacterActionAsset[] _startupActions;

        private CancellationTokenSource _enableCts;

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
                await _startupActions[i].Apply(_characterAccess, this, token);
            }
        }
    }

}
