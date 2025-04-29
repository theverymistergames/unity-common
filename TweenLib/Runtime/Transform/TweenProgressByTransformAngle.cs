using System;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public sealed class TweenProgressByTransformAngle : ITweenProgress {
        
        public Transform transform;
        public Vector3 angleOffset;
        public Transform target;
        public Vector3 targetAngleOffset;
        [Range(0f, 180f)] public float maxAngle = 180f;
        
        public float GetProgress() {
            var forward = transform.rotation * Quaternion.Euler(angleOffset) * Vector3.forward;
            var targetForward = target.rotation * Quaternion.Euler(targetAngleOffset) * Vector3.forward;
            
            float angle = Vector3.Angle(forward, targetForward);
            
            return 1f - Mathf.Clamp01(angle / maxAngle);
        }
    }
    
}