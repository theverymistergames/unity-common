using System;
using System.Buffers;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Common.Lists;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public struct SceneTransaction {

        [SerializeField] private SceneReference[] _load;
        [SerializeField] private SceneReference[] _unload;

        [SerializeField] private bool _activateSceneAfterLoad;

        [VisibleIf(nameof(_activateSceneAfterLoad))]
        [SerializeField] private SceneReference _targetActiveScene;

        public async UniTask Apply(CancellationToken cancellationToken = default) {
            int loadLength = _load.Length;
            int operationsCount = loadLength + _unload.Length;

            if (operationsCount <= 0) return;

            var tasks = ArrayPool<UniTask>.Shared.Rent(operationsCount);

            for (int i = 0; i < operationsCount; i++) {
                tasks[i] = i < loadLength
                    ? SceneLoader.LoadSceneAsync(_load[i].scene, false)
                    : SceneLoader.UnloadSceneAsync(_unload[i - loadLength].scene);
            }

            await UniTask.WhenAll(tasks);

            tasks.ResetArrayElements();
            ArrayPool<UniTask>.Shared.Return(tasks);
            
            if (cancellationToken.IsCancellationRequested) return;
            
            if (_activateSceneAfterLoad) await SceneLoader.LoadSceneAsync(_targetActiveScene.scene, true);
        }
    }

}
