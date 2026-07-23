using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Service;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.SceneRoots;

namespace MisterGames.ActionLib.Scenes {
    
    [Serializable]
    public sealed class SetSceneRootEnabledAction : IActorAction {
        
        public SceneReference scene;
        public bool enabled;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (Services.TryGet(out ISceneRootService sceneRootService)) {
                sceneRootService.SetSceneRootEnabled(scene.scene, enabled);
            }
            
            return default;
        }
    }
    
}