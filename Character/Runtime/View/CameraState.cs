using UnityEngine;

namespace MisterGames.Character.View {

    public readonly struct CameraState {

        public readonly int hash;
        public readonly float weight;

        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly float fov;

        public CameraState(int hash, float weight) {
            this.hash = hash;
            this.weight = weight;

            position = Vector3.zero;
            rotation = Quaternion.identity;
            fov = 0f;
        }

        public CameraState(int hash, float weight, Vector3 position, Quaternion rotation, float fov) {
            this.hash = hash;
            this.weight = weight;

            this.position = position;
            this.rotation = rotation;
            this.fov = fov;
        }

        public CameraState WithWeight(float value) => new CameraState(hash, value, position, rotation, fov);
        public CameraState WithPosition(Vector3 value) => new CameraState(hash, weight, value, rotation, fov);
        public CameraState WithRotation(Quaternion value) => new CameraState(hash, weight, position, value, fov);
        public CameraState WithFovOffset(float value) => new CameraState(hash, weight, position, rotation, value);
    }

}
