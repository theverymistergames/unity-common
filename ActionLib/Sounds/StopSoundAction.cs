using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Data;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class StopSoundAction : IActorAction {

        public HashId attachId;
        public PlaySoundAction.PositionMode position;
        [VisibleIf(nameof(position), 1)]
        public Transform transform;
        [VisibleIf(nameof(position), 2)]
        public LabelValue<UnityEngine.Object> libraryObject;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (AudioPool.Main is not { } pool) return default;
            
            var trf = position switch {
                PlaySoundAction.PositionMode.ActorTransform => context.Transform,
                PlaySoundAction.PositionMode.ExplicitTransform => transform,
                PlaySoundAction.PositionMode.LibraryObject => libraryObject.TryGetData(out var obj) && obj is Component c ? c.transform : null,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            pool.GetAudioHandle(trf, attachId).Release();
            
            return default;
        }
    }
    
}