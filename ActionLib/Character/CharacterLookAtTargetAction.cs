using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterLookAtTargetAction : IActorAction {

        public Transform target;
        public LookAtMode mode;
        [VisibleIf(nameof(mode), 1)] public Vector3 orientation;
        [Min(0f)] public float angularSpeed;
        [Min(0f)] public float maxAngle;
        public bool keepLookingAtAfterFinish;
        [Min(0f)] public float smoothing;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var view = context.GetComponent<CharacterViewPipeline>();
            var head = context.GetComponent<CharacterHeadAdapter>();

            var timeSource = PlayerLoopStage.Update.Get();
            
            while (!cancellationToken.IsCancellationRequested) {
                var targetRotation = Quaternion.LookRotation(target.position - head.Position);
                var rotation = Quaternion.SlerpUnclamped(head.Rotation, targetRotation, timeSource.DeltaTime * angularSpeed);
                
                float angle = Quaternion.Angle(targetRotation, rotation);
                if (angle <= maxAngle) break;
                
                view.LookAt(head.Position + rotation * Vector3.forward);
                
                await UniTask.Yield();
            }
            
            if (cancellationToken.IsCancellationRequested) return;
            
            if (keepLookingAtAfterFinish) view.LookAt(target, mode, orientation, smoothing);
            else view.StopLookAt();
        }
    }
    
}