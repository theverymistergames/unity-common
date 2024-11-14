using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Tick.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace MisterGames.ActionLib.Character {
    /*
    [Serializable]
    public sealed class DepthOfFieldWeightAction : IActorAction {
        
        public VolumeProfile volumeProfile;
        [Range(0f, 1f)] public float weight;
        [Min(0f)] public float duration;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (!volumeProfile.TryGet(out DepthOfField depthOfField)) return;
            
            float t = 0f;
            float speed = duration > 0f ? 1f : float.MaxValue;
            var ts = PlayerLoopStage.Update.Get();
            
            while (!cancellationToken.IsCancellationRequested) {
                t = Mathf.Clamp01(t + speed * ts.DeltaTime);

                depthOfField.farFocusStart.min = 0f;
                
                await UniTask.Yield();
            }
        }
    }
    */
}