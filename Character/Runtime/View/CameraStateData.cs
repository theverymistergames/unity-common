using UnityEngine;

namespace MisterGames.Character.View {

    public readonly struct CameraStateData {

        public readonly float weight;
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly float fov;

        public CameraStateData(float weight) {
            this.weight = weight;

            position = Vector3.zero;
            rotation = Quaternion.identity;
            fov = 0f;
        }

        public CameraStateData(float weight, Vector3 position, Quaternion rotation, float fov) {
            this.weight = weight;
            this.position = position;
            this.rotation = rotation;
            this.fov = fov;
        }

        public CameraStateData WithPosition(Vector3 value) => new CameraStateData(weight, value, rotation, fov);
        public CameraStateData WithRotation(Quaternion value) => new CameraStateData(weight, position, value, fov);
        public CameraStateData WithFovOffset(float value) => new CameraStateData(weight, position, rotation, value);
    }

}
