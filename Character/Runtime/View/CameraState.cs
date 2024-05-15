using UnityEngine;

namespace MisterGames.Character.View {
    
    public readonly struct CameraState {
        
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly float fov;
        
        public CameraState(Vector3 position, Quaternion rotation, float fov) {
            this.position = position;
            this.rotation = rotation;
            this.fov = fov;
        }

        public static readonly CameraState Empty = new CameraState(Vector3.zero, Quaternion.identity, 0f);

        public CameraState WithPosition(Vector3 position) => new CameraState(position, rotation, fov);
        public CameraState WithRotation(Quaternion rotation) => new CameraState(position, rotation, fov);
        public CameraState WithFov(float fov) => new CameraState(position, rotation, fov);
    }
    
}