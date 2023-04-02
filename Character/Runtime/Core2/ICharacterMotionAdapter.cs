using UnityEngine;

namespace MisterGames.Character.Core2 {

    public interface ICharacterMotionAdapter {
        Vector3 Position { get; }
        Quaternion Rotation { get; }

        void Move(Vector3 delta);
        void Rotate(Quaternion delta);
    }

}
