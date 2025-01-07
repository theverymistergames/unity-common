using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class TransformScaleCondition : IActorCondition {

        public Transform target;
        public ComparedValue compareValue;
        public CompareMode mode;
        public float value;
        
        public enum ComparedValue {
            Magnitude,
            AxisX,
            AxisY,
            AxisZ,
        }
        
        public bool IsMatch(IActor context, float startTime) {
            var scale = target.localScale;
            return compareValue switch {
                ComparedValue.Magnitude => mode.IsMatch(scale.sqrMagnitude, 3f * Mathf.Sign(value) * value * value),
                ComparedValue.AxisX => mode.IsMatch(scale.x, value),
                ComparedValue.AxisY => mode.IsMatch(scale.y, value),
                ComparedValue.AxisZ => mode.IsMatch(scale.z, value),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
}