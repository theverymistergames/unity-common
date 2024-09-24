using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CameraShakeAction : IActorAction {

        public float weight = 1f;
        [Min(0f)] public float duration;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public FloatParameter noiseScale = FloatParameter.Default();
        public Vector3Parameter positionMultiplier = Vector3Parameter.Default();
        public Vector3Parameter rotationMultiplier = Vector3Parameter.Default();
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (!context.TryGetComponent(out CameraShaker shaker)) return;

            int id = shaker.CreateState(weight);

            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            var ts = PlayerLoopStage.Update.Get();

            float m = noiseScale.CreateMultiplier();
            var pm = positionMultiplier.CreateMultiplier();
            var rm = rotationMultiplier.CreateMultiplier();
            
            while (!cancellationToken.IsCancellationRequested && t < 1f) {
                t = Mathf.Clamp01(t + speed * ts.DeltaTime);

                float scale = m * noiseScale.Evaluate(t);
                var pos = pm.Multiply(positionMultiplier.Evaluate(t));
                var rot = rm.Multiply(rotationMultiplier.Evaluate(t));
                
                shaker.SetNoiseScale(id, scale);
                shaker.SetPosition(id, positionOffset, pos);
                shaker.SetRotation(id, rotationOffset, rot);
                
                await UniTask.Yield();
            }
            
            shaker.RemoveState(id);
        }
    }
    
}