using System;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(Vector2))]
    public sealed class SaveTableVector2 : SaveTable<Vector2> {}

    [Serializable]
    [SaveTable(typeof(Vector3))]
    public sealed class SaveTableVector3 : SaveTable<Vector3> {}

    [Serializable]
    [SaveTable(typeof(Vector4))]
    public sealed class SaveTableVector4 : SaveTable<Vector4> {}

    [Serializable]
    [SaveTable(typeof(Vector2Int))]
    public sealed class SaveTableVector2Int : SaveTable<Vector2Int> {}

    [Serializable]
    [SaveTable(typeof(Vector3Int))]
    public sealed class SaveTableVector3Int : SaveTable<Vector3Int> {}

    [Serializable]
    [SaveTable(typeof(Quaternion))]
    public sealed class SaveTableQuaternion : SaveTable<Quaternion> {}

    [Serializable]
    [SaveTable(typeof(LayerMask))]
    public sealed class SaveTableLayerMask : SaveTable<LayerMask> {}

    [Serializable]
    [SaveTable(typeof(Color))]
    public sealed class SaveTableColor : SaveTable<Color> {}

    [Serializable]
    [SaveTable(typeof(AnimationCurve))]
    public sealed class SaveTableAnimationCurve : SaveTable<AnimationCurve> {}

}
