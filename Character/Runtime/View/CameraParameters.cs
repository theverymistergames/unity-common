using UnityEngine;

namespace MisterGames.Character.View {

    public readonly struct CameraParameters {

        public static readonly CameraParameters Default = new CameraParameters(Vector3.zero, Quaternion.identity, 0f);

        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly float fov;

        public CameraParameters(Vector3 position, Quaternion rotation, float fov) {
            this.position = position;
            this.rotation = rotation;
            this.fov = fov;
        }

        public CameraParameters WithPosition(Vector3 value) => new CameraParameters(value, rotation, fov);
        public CameraParameters WithRotation(Quaternion value) => new CameraParameters(position, value, fov);
        public CameraParameters WithFovOffset(float value) => new CameraParameters(position, rotation, value);
    }

}
