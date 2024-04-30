using System;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(Vector2[]))]
    public sealed class SaveTableVector2Array : SaveTable<Vector2[]> {}

    [Serializable]
    [SaveTable(typeof(Vector3[]))]
    public sealed class SaveTableVector3Array : SaveTable<Vector3[]> {}

    [Serializable]
    [SaveTable(typeof(Vector4[]))]
    public sealed class SaveTableVector4Array : SaveTable<Vector4[]> {}

    [Serializable]
    [SaveTable(typeof(Vector2Int[]))]
    public sealed class SaveTableVector2IntArray : SaveTable<Vector2Int[]> {}

    [Serializable]
    [SaveTable(typeof(Vector3Int[]))]
    public sealed class SaveTableVector3IntArray : SaveTable<Vector3Int[]> {}

    [Serializable]
    [SaveTable(typeof(Quaternion[]))]
    public sealed class SaveTableQuaternionArray : SaveTable<Quaternion[]> {}

    [Serializable]
    [SaveTable(typeof(LayerMask[]))]
    public sealed class SaveTableLayerMaskArray : SaveTable<LayerMask[]> {}

    [Serializable]
    [SaveTable(typeof(Color[]))]
    public sealed class SaveTableColorArray : SaveTable<Color[]> {}

    [Serializable]
    [SaveTable(typeof(AnimationCurve[]))]
    public sealed class SaveTableAnimationCurveArray : SaveTable<AnimationCurve[]> {}

}
