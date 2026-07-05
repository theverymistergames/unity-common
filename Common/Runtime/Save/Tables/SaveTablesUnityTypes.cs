using System;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(int), typeof(Vector2))]
    public sealed class SaveTableIntVector2 : SaveTable<int, Vector2> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Vector2))]
    public sealed class SaveTableLongVector2 : SaveTable<long, Vector2> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Vector2))]
    public sealed class SaveTableStringVector2 : SaveTable<string, Vector2> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Vector2))]
    public sealed class SaveTableSaveKeyVector2 : SaveTable<SaveKey, Vector2> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Vector3))]
    public sealed class SaveTableIntVector3 : SaveTable<int, Vector3> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Vector3))]
    public sealed class SaveTableLongVector3 : SaveTable<long, Vector3> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Vector3))]
    public sealed class SaveTableStringVector3 : SaveTable<string, Vector3> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Vector3))]
    public sealed class SaveTableSaveKeyVector3 : SaveTable<SaveKey, Vector3> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Vector4))]
    public sealed class SaveTableIntVector4 : SaveTable<int, Vector4> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Vector4))]
    public sealed class SaveTableLongVector4 : SaveTable<long, Vector4> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Vector4))]
    public sealed class SaveTableStringVector4 : SaveTable<string, Vector4> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Vector4))]
    public sealed class SaveTableSaveKeyVector4 : SaveTable<SaveKey, Vector4> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Vector2Int))]
    public sealed class SaveTableIntVector2Int : SaveTable<int, Vector2Int> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Vector2Int))]
    public sealed class SaveTableLongVector2Int : SaveTable<long, Vector2Int> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Vector2Int))]
    public sealed class SaveTableStringVector2Int : SaveTable<string, Vector2Int> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Vector2Int))]
    public sealed class SaveTableSaveKeyVector2Int : SaveTable<SaveKey, Vector2Int> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Vector3Int))]
    public sealed class SaveTableIntVector3Int : SaveTable<int, Vector3Int> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Vector3Int))]
    public sealed class SaveTableLongVector3Int : SaveTable<long, Vector3Int> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Vector3Int))]
    public sealed class SaveTableStringVector3Int : SaveTable<string, Vector3Int> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Vector3Int))]
    public sealed class SaveTableSaveKeyVector3Int : SaveTable<SaveKey, Vector3Int> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Quaternion))]
    public sealed class SaveTableIntQuaternion : SaveTable<int, Quaternion> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Quaternion))]
    public sealed class SaveTableLongQuaternion : SaveTable<long, Quaternion> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Quaternion))]
    public sealed class SaveTableStringQuaternion : SaveTable<string, Quaternion> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Quaternion))]
    public sealed class SaveTableSaveKeyQuaternion : SaveTable<SaveKey, Quaternion> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(LayerMask))]
    public sealed class SaveTableIntLayerMask : SaveTable<int, LayerMask> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(LayerMask))]
    public sealed class SaveTableLongLayerMask : SaveTable<long, LayerMask> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(LayerMask))]
    public sealed class SaveTableStringLayerMask : SaveTable<string, LayerMask> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(LayerMask))]
    public sealed class SaveTableSaveKeyLayerMask : SaveTable<SaveKey, LayerMask> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Color))]
    public sealed class SaveTableIntColor : SaveTable<int, Color> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Color))]
    public sealed class SaveTableLongColor : SaveTable<long, Color> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Color))]
    public sealed class SaveTableStringColor : SaveTable<string, Color> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Color))]
    public sealed class SaveTableSaveKeyColor : SaveTable<SaveKey, Color> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(AnimationCurve))]
    public sealed class SaveTableIntAnimationCurve : SaveTable<int, AnimationCurve> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(AnimationCurve))]
    public sealed class SaveTableLongAnimationCurve : SaveTable<long, AnimationCurve> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(AnimationCurve))]
    public sealed class SaveTableStringAnimationCurve : SaveTable<string, AnimationCurve> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(AnimationCurve))]
    public sealed class SaveTableSaveKeyAnimationCurve : SaveTable<SaveKey, AnimationCurve> {}

}
