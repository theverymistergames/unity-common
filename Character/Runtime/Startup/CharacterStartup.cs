using System.Threading;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Startup {

    public sealed class CharacterStartup : MonoBehaviour {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private AsyncActionAsset[] _startupActions;

        [RuntimeDependency(typeof(CharacterAccess))]
        [FetchDependencies(nameof(_startupActions))]
        [SerializeField] private DependencyResolver _dependencies;

        private CancellationTokenSource _enableCts;

        private void Awake() {
            _dependencies.SetValue(_characterAccess);
            _dependencies.Resolve(_startupActions);
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

        private async void Start() {
            for (int i = 0; i < _startupActions.Length; i++) {
                await _startupActions[i].Apply(this, _enableCts.Token);
            }
        }
    }

}
