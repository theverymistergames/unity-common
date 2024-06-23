using System;
using UnityEngine;

namespace MisterGames.Common.GameObjects {
    
    [Serializable]
    public struct TransformData {
        
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public static readonly TransformData Default = new TransformData(Vector3.zero, Quaternion.identity, Vector3.one);
        
        public TransformData(Vector3 position, Quaternion rotation, Vector3 scale) {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public TransformData WithPosition(Vector3 position) {
            this.position = position;
            return this;
        }
        
        public TransformData WithRotation(Quaternion rotation) {
            this.rotation = rotation;
            return this;
        }
        
        public TransformData WithScale(Vector3 scale) {
            this.scale = scale;
            return this;
        }
    }
    
}