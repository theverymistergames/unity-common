using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common.Attributes;
using MisterGames.Common.Lists;
using MisterGames.Tick.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class StopSoundAction : IActorAction {
        
        public AudioSource source;
        [Min(0f)] public float fadeOut = 0.3f;
        public AnimationCurve fadeOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (fadeOut <= 0f) {
                source.Stop();
                return;
            }
            
            float t = 0f;
            float speed = fadeOut > 0f ? 1f / fadeOut : float.MaxValue;
            float startVolume = source.volume;
            var timeSource = PlayerLoopStage.Update.Get();
            
            while (!cancellationToken.IsCancellationRequested) {
                t = Mathf.Clamp01(t + timeSource.DeltaTime * speed);
                source.volume = Mathf.Lerp(startVolume, 0f, fadeOutCurve.Evaluate(t));
                
                if (t >= 1f) break;
                
                await UniTask.Yield();
            }

            if (!cancellationToken.IsCancellationRequested) source.Stop();
        }
    }
    
}