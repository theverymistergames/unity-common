﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterLookAtTargetAction : IActorAction {

        public Transform target;
        public LookAtMode mode;
        [VisibleIf(nameof(mode), 1)] public Vector3 orientation;
        [Min(0f)] public float duration = 0.5f;
        public bool keepLookingAtAfterFinish;
        [Min(0f)] public float attachSmoothing;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var view = context.GetComponent<CharacterViewPipeline>();

            var rotation = view.HeadRotation;
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            
            while (!cancellationToken.IsCancellationRequested) {
                var targetRotation = mode switch {
                    LookAtMode.Free => Quaternion.LookRotation(target.position - view.HeadPosition, Vector3.up),
                    LookAtMode.Oriented => target.rotation * Quaternion.Euler(orientation),
                    _ => throw new ArgumentOutOfRangeException()
                };

                t += UnityEngine.Time.deltaTime * speed;
                rotation = Quaternion.Slerp(rotation, targetRotation, t);
                
                if (t >= 1f) break;
                
                view.LookAt(view.HeadPosition + rotation * Vector3.forward);
                
#if UNITY_EDITOR
                DebugExt.DrawRay(view.HeadPosition, view.HeadRotation * Vector3.forward, Color.yellow);
                DebugExt.DrawRay(view.HeadPosition, targetRotation * Vector3.forward, Color.green);          
#endif
                
                await UniTask.Yield();
            }
            
            if (cancellationToken.IsCancellationRequested) return;
            
            if (keepLookingAtAfterFinish) view.LookAt(target, mode, orientation, attachSmoothing);
            else view.StopLookAt();
        }
    }
    
}