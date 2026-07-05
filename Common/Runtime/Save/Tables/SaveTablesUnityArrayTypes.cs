using System;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(int), typeof(Vector2[]))]
    public sealed class SaveTableIntVector2Array : SaveTable<int, Vector2[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Vector2[]))]
    public sealed class SaveTableLongVector2Array : SaveTable<long, Vector2[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Vector2[]))]
    public sealed class SaveTableStringVector2Array : SaveTable<string, Vector2[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Vector2[]))]
    public sealed class SaveTableSaveKeyVector2Array : SaveTable<SaveKey, Vector2[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Vector3[]))]
    public sealed class SaveTableIntVector3Array : SaveTable<int, Vector3[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Vector3[]))]
    public sealed class SaveTableLongVector3Array : SaveTable<long, Vector3[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Vector3[]))]
    public sealed class SaveTableStringVector3Array : SaveTable<string, Vector3[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Vector3[]))]
    public sealed class SaveTableSaveKeyVector3Array : SaveTable<SaveKey, Vector3[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Vector4[]))]
    public sealed class SaveTableIntVector4Array : SaveTable<int, Vector4[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Vector4[]))]
    public sealed class SaveTableLongVector4Array : SaveTable<long, Vector4[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Vector4[]))]
    public sealed class SaveTableStringVector4Array : SaveTable<string, Vector4[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Vector4[]))]
    public sealed class SaveTableSaveKeyVector4Array : SaveTable<SaveKey, Vector4[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Vector2Int[]))]
    public sealed class SaveTableIntVector2IntArray : SaveTable<int, Vector2Int[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Vector2Int[]))]
    public sealed class SaveTableLongVector2IntArray : SaveTable<long, Vector2Int[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Vector2Int[]))]
    public sealed class SaveTableStringVector2IntArray : SaveTable<string, Vector2Int[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Vector2Int[]))]
    public sealed class SaveTableSaveKeyVector2IntArray : SaveTable<SaveKey, Vector2Int[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Vector3Int[]))]
    public sealed class SaveTableIntVector3IntArray : SaveTable<int, Vector3Int[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Vector3Int[]))]
    public sealed class SaveTableLongVector3IntArray : SaveTable<long, Vector3Int[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Vector3Int[]))]
    public sealed class SaveTableStringVector3IntArray : SaveTable<string, Vector3Int[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Vector3Int[]))]
    public sealed class SaveTableSaveKeyVector3IntArray : SaveTable<SaveKey, Vector3Int[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Quaternion[]))]
    public sealed class SaveTableIntQuaternionArray : SaveTable<int, Quaternion[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Quaternion[]))]
    public sealed class SaveTableLongQuaternionArray : SaveTable<long, Quaternion[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Quaternion[]))]
    public sealed class SaveTableStringQuaternionArray : SaveTable<string, Quaternion[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Quaternion[]))]
    public sealed class SaveTableSaveKeyQuaternionArray : SaveTable<SaveKey, Quaternion[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(LayerMask[]))]
    public sealed class SaveTableIntLayerMaskArray : SaveTable<int, LayerMask[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(LayerMask[]))]
    public sealed class SaveTableLongLayerMaskArray : SaveTable<long, LayerMask[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(LayerMask[]))]
    public sealed class SaveTableStringLayerMaskArray : SaveTable<string, LayerMask[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(LayerMask[]))]
    public sealed class SaveTableSaveKeyLayerMaskArray : SaveTable<SaveKey, LayerMask[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(Color[]))]
    public sealed class SaveTableIntColorArray : SaveTable<int, Color[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(Color[]))]
    public sealed class SaveTableLongColorArray : SaveTable<long, Color[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(Color[]))]
    public sealed class SaveTableStringColorArray : SaveTable<string, Color[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Color[]))]
    public sealed class SaveTableSaveKeyColorArray : SaveTable<SaveKey, Color[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(AnimationCurve[]))]
    public sealed class SaveTableIntAnimationCurveArray : SaveTable<int, AnimationCurve[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(AnimationCurve[]))]
    public sealed class SaveTableLongAnimationCurveArray : SaveTable<long, AnimationCurve[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(AnimationCurve[]))]
    public sealed class SaveTableStringAnimationCurveArray : SaveTable<string, AnimationCurve[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(AnimationCurve[]))]
    public sealed class SaveTableSaveKeyAnimationCurveArray : SaveTable<SaveKey, AnimationCurve[]> {}

}
