using UnityEngine;

namespace MisterGames.Common.GameObjects {

    public interface ITransformAdapter {
        Vector3 Position { get; set; }
        Vector3 LocalPosition { get; set; }
        Quaternion Rotation { get; set; }
        Quaternion LocalRotation { get; set; }

        void Move(Vector3 delta);
        void Rotate(Quaternion delta);
    }

}
