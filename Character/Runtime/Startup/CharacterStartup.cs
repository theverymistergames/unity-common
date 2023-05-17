using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Startup {

    public sealed class CharacterStartup : MonoBehaviour {

        [SerializeField] private AsyncActionAsset[] _startupActions;

        [FetchDependencies(nameof(_startupActions))]
        [SerializeField] private DependencyResolver _dependencies;

        private CancellationTokenSource _enableCts;
        private UniTask[] _tasks;

        private void Awake() {
            _tasks = new UniTask[_startupActions.Length];
            _dependencies.Resolve(_startupActions);

            for (int i = 0; i < _startupActions.Length; i++) {
                _startupActions[i].Initialize();
            }
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
                _tasks[i] = _startupActions[i].Apply(this, _enableCts.Token);
            }

            await UniTask.WhenAll(_tasks);

            for (int i = 0; i < _startupActions.Length; i++) {
                _startupActions[i].DeInitialize();
            }
        }
    }

}
