using UnityEngine;

namespace MisterGames.Splines.Tools {
    
    public readonly struct MeshVertex {
        
        public readonly Vector3 position;
        public readonly Vector3 normal;
        public readonly Vector2 uv;

        public MeshVertex(Vector3 position, Vector3 normal, Vector2 uv) {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
        }
    }

}
