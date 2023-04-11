using UnityEngine;

namespace MisterGames.Character.Core2.Motion {

    public interface ITransformAdapter {
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }

        void Move(Vector3 delta);
        void Rotate(Quaternion delta);
    }

}
