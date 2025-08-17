using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Core;

namespace MisterGames.ActionLib.Scenes {
    
    [Serializable]
    public sealed class SceneLoadAction : IActorAction {

        public SceneReference scene;
        public Mode mode;
        [VisibleIf(nameof(mode), 0)]
        public bool makeActive;
        
        public enum Mode {
            Load,
            Unload,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return mode switch {
                Mode.Load => SceneLoader.LoadSceneAsync(scene.scene, makeActive),
                Mode.Unload => SceneLoader.UnloadSceneAsync(scene.scene),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
}