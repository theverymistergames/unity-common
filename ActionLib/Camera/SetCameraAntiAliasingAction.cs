using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine.Rendering.HighDefinition;

namespace MisterGames.ActionLib.Camera {
    
    [Serializable]
    public sealed class SetCameraAntiAliasingAction : IActorAction {
    
        public UnityEngine.Camera camera;
        public HDAdditionalCameraData.AntialiasingMode mode;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var hd = camera.GetComponent<HDAdditionalCameraData>();

            hd.antialiasing = mode;
            return UniTask.CompletedTask;
        }
    }
    
}