using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public sealed class SceneTransaction : IAsyncAction {

        [SerializeField] private SceneReference[] _load;
        [SerializeField] private SceneReference[] _unload;

        [SerializeField] private bool _activateSceneAfterLoad;

        [VisibleIf(nameof(_activateSceneAfterLoad))]
        [SerializeField] private SceneReference _targetActiveScene;

        private UniTask[] _tasks;

        public async UniTask Apply(object source, CancellationToken cancellationToken = default) {
            int loadLength = _load.Length;
            int operationsCount = loadLength + _unload.Length;

            _tasks ??= operationsCount > 0 ? new UniTask[operationsCount] : Array.Empty<UniTask>();

            for (int i = 0; i < operationsCount; i++) {
                _tasks[i] = i < loadLength
                    ? SceneLoader.LoadSceneAsync(_load[i].scene, false)
                    : SceneLoader.UnloadSceneAsync(_unload[i - loadLength].scene);
            }

            await UniTask.WhenAll(_tasks);

            if (_activateSceneAfterLoad) await SceneLoader.LoadSceneAsync(_targetActiveScene.scene, true);
        }
    }

}
