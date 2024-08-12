using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common;
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
            var rotation = head.Rotation;
            
            while (!cancellationToken.IsCancellationRequested) {
                var targetRotation = mode switch {
                    LookAtMode.Free => Quaternion.LookRotation(target.position - head.Position, Vector3.up),
                    LookAtMode.Oriented => target.rotation * Quaternion.Euler(orientation),
                    _ => throw new ArgumentOutOfRangeException()
                };

                rotation = Quaternion.RotateTowards(rotation, targetRotation, timeSource.DeltaTime * angularSpeed);
                
                float angle = Quaternion.Angle(targetRotation, rotation);
                if (angle <= maxAngle) break;
                
                view.LookAt(head.Position + rotation * Vector3.forward);
                
#if UNITY_EDITOR
                DebugExt.DrawRay(head.Position, head.Rotation * Vector3.forward, Color.yellow);
                DebugExt.DrawRay(head.Position, targetRotation * Vector3.forward, Color.green);          
#endif
                
                await UniTask.Yield();
            }
            
            if (cancellationToken.IsCancellationRequested) return;
            
            if (keepLookingAtAfterFinish) view.LookAt(target, mode, orientation, smoothing);
            else view.StopLookAt();
        }
    }
    
}