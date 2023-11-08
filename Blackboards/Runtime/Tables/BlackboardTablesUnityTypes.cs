using System;
using UnityEngine;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    [BlackboardTable(typeof(Vector2))]
    public sealed class BlackboardTableVector2 : BlackboardTable<Vector2> {}

    [Serializable]
    [BlackboardTable(typeof(Vector3))]
    public sealed class BlackboardTableVector3 : BlackboardTable<Vector3> {}

    [Serializable]
    [BlackboardTable(typeof(Vector4))]
    public sealed class BlackboardTableVector4 : BlackboardTable<Vector4> {}

    [Serializable]
    [BlackboardTable(typeof(Vector2Int))]
    public sealed class BlackboardTableVector2Int : BlackboardTable<Vector2Int> {}

    [Serializable]
    [BlackboardTable(typeof(Vector3Int))]
    public sealed class BlackboardTableVector3Int : BlackboardTable<Vector3Int> {}

    [Serializable]
    [BlackboardTable(typeof(Quaternion))]
    public sealed class BlackboardTableQuaternion : BlackboardTable<Quaternion> {}

    [Serializable]
    [BlackboardTable(typeof(LayerMask))]
    public sealed class BlackboardTableLayerMask : BlackboardTable<LayerMask> {}

    [Serializable]
    [BlackboardTable(typeof(Color))]
    public sealed class BlackboardTableColor : BlackboardTable<Color> {}

    [Serializable]
    [BlackboardTable(typeof(AnimationCurve))]
    public sealed class BlackboardTableAnimationCurve : BlackboardTable<AnimationCurve> {}

}
