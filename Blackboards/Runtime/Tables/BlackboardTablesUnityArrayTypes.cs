using System;
using UnityEngine;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    [BlackboardTable(typeof(Vector2[]))]
    public sealed class BlackboardTableVector2Array : BlackboardTable<Vector2[]> {}

    [Serializable]
    [BlackboardTable(typeof(Vector3[]))]
    public sealed class BlackboardTableVector3Array : BlackboardTable<Vector3[]> {}

    [Serializable]
    [BlackboardTable(typeof(Vector4[]))]
    public sealed class BlackboardTableVector4Array : BlackboardTable<Vector4[]> {}

    [Serializable]
    [BlackboardTable(typeof(Vector2Int[]))]
    public sealed class BlackboardTableVector2IntArray : BlackboardTable<Vector2Int[]> {}

    [Serializable]
    [BlackboardTable(typeof(Vector3Int[]))]
    public sealed class BlackboardTableVector3IntArray : BlackboardTable<Vector3Int[]> {}

    [Serializable]
    [BlackboardTable(typeof(Quaternion[]))]
    public sealed class BlackboardTableQuaternionArray : BlackboardTable<Quaternion[]> {}

    [Serializable]
    [BlackboardTable(typeof(LayerMask[]))]
    public sealed class BlackboardTableLayerMaskArray : BlackboardTable<LayerMask[]> {}

    [Serializable]
    [BlackboardTable(typeof(Color[]))]
    public sealed class BlackboardTableColorArray : BlackboardTable<Color[]> {}

    [Serializable]
    [BlackboardTable(typeof(AnimationCurve[]))]
    public sealed class BlackboardTableAnimationCurveArray : BlackboardTable<AnimationCurve[]> {}

}
