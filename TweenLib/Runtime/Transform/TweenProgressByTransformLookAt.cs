using System;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public sealed class TweenProgressByTransformLookAt : ITweenProgress {
        
        public Transform transform;
        public Vector3 angleOffset;
        public Transform lookAt;
        [Range(0f, 180f)] public float maxAngle = 180f;
        
        public float GetProgress() {
            var forward = transform.rotation * Quaternion.Euler(angleOffset) * Vector3.forward;
            var targetForward = lookAt.position - transform.position;
            
            float angle = Vector3.Angle(forward, targetForward);
            
            return 1f - Mathf.Clamp01(angle / maxAngle);
        }
    }
    
}