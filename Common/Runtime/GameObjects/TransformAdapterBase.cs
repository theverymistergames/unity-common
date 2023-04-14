using UnityEngine;

namespace MisterGames.Common.GameObjects {

    public abstract class TransformAdapterBase : MonoBehaviour, ITransformAdapter {
        public abstract Vector3 Position { get; set; }
        public abstract Quaternion Rotation { get; set; }

        public abstract void Move(Vector3 delta);
        public abstract void Rotate(Quaternion delta);
    }

}
