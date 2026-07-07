using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Interactives;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CameraShakeWeightAction : IActorAction {

        public CameraShakeBehaviour cameraShakeBehaviour;
        [Range(0f, 1f)] public float weight = 1f;
        public Optional<float> smoothing;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            cameraShakeBehaviour.SetWeight(weight);
            if (smoothing.HasValue) cameraShakeBehaviour.SetSmoothing(smoothing.Value);
            
            return default;
        }
    }
    
}