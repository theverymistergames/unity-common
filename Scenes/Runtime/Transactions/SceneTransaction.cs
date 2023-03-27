using System;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public sealed class SceneTransaction {

        [SerializeField] private SceneReference[] _load;
        [SerializeField] private SceneReference[] _unload;

        [SerializeField] private bool _activateSceneAfterLoad;

        [VisibleIf(nameof(_activateSceneAfterLoad))]
        [SerializeField] private SceneReference _targetActiveScene;

        [VisibleIf(nameof(_activateSceneAfterLoad))]
        //[ReadOnly]
        [SerializeField] private AnimationCurve _c;

        public async UniTask Commit() {
            int loadLength = _load.Length;
            int operationsCount = loadLength + _unload.Length;
            var tasks = operationsCount > 0 ? new UniTask[operationsCount] : Array.Empty<UniTask>();

            for (int i = 0; i < operationsCount; i++) {
                tasks[i] = i < loadLength
                    ? SceneLoader.LoadSceneAsync(_load[i].scene, false)
                    : SceneLoader.UnloadSceneAsync(_unload[i - loadLength].scene);
            }

            await UniTask.WhenAll(tasks);

            if (_activateSceneAfterLoad) await SceneLoader.LoadSceneAsync(_targetActiveScene.scene, true);
        }
    }

}
