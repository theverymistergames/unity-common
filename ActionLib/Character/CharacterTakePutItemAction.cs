using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Easing;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterTakePutItemAction : IActorAction {
        
        public Transform item;
        public Mode method = Mode.Take;
        [Min(0f)] public float duration = 0.3f;
        public AnimationCurve curve = EasingType.EaseInCubic.ToAnimationCurve();
        public bool disableCollider = true;
    
        public enum Mode {
            Take,
            Put,
        }
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var transform = context.Transform;
            
            Collider[] colliders = null;

            if (disableCollider) {
                colliders = item.GetComponentsInChildren<Collider>(includeInactive: true);
                colliders.SetEnabled(false);
            }

            var start = method == Mode.Take ? item.position : transform.position;
            var end = method == Mode.Put ? item.position : transform.position;
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            
            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                t = Mathf.Clamp01(t + UnityEngine.Time.deltaTime * speed);
                item.position = Vector3.Lerp(start, end, curve.Evaluate(t));
                
                await UniTask.Yield();
            }
            
            switch (method) {
                case Mode.Take:
                    item.gameObject.SetActive(false);
                    break;
                
                case Mode.Put: {
                    if (disableCollider) colliders.SetEnabled(true);
                    break;
                }
            }
        }
    }
    
}