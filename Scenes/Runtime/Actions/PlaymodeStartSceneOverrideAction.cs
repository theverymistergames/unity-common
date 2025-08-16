using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Lists;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Utils;
using UnityEngine;

namespace MisterGames.Scenes.Actions {
    
    [Serializable]
    public sealed class PlaymodeStartSceneOverrideAction : ISceneLoaderAction {
        
        [SerializeField] private SceneReference _startScene;
        [SerializeField] private SceneReference _preloadIfNotStartScene;
        
        public async UniTask Apply(CancellationToken cancellationToken) {
#if UNITY_EDITOR
            if (!PlaymodeStartScenesUtils.IsPlaymodeStartScenesOverrideEnabled(out var playmodeStartScenes)) return;

            // Force load gameplay scene in Unity Editor's playmode
            // if app is launched from custom scene.
            if (!playmodeStartScenes.Contains(_startScene.scene)) {
                SceneLoader.LaunchMode = ApplicationLaunchMode.FromCustomEditorScene;

                if (_preloadIfNotStartScene.IsValid()) {
                    await SceneLoader.LoadSceneAsync(_preloadIfNotStartScene.scene, makeActive: false);
                    if (cancellationToken.IsCancellationRequested) return;   
                }
            }

            await SceneLoader.LoadScenesAsync(playmodeStartScenes, activeScene: playmodeStartScenes[0]);
#endif
        }
    }
    
}