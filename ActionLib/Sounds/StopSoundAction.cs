using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class StopSoundAction : IActorAction {

        public HashId attachId;
        public PlaySoundAction.PositionMode position;
        [VisibleIf(nameof(position), 1)]
        public Transform transform;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (AudioPool.Main is not { } pool) return default;
            
            var trf = position switch {
                PlaySoundAction.PositionMode.ActorTransform => context.Transform,
                PlaySoundAction.PositionMode.ExplicitTransform => transform,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            pool.GetAudioHandle(trf, attachId).Release();
            
            return default;
        }
    }
    
}