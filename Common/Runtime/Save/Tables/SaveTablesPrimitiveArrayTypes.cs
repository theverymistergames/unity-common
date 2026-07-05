using System;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(int), typeof(bool[]))]
    public sealed class SaveTableIntBoolArray : SaveTable<int, bool[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(bool[]))]
    public sealed class SaveTableLongBoolArray : SaveTable<long, bool[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(bool[]))]
    public sealed class SaveTableStringBoolArray : SaveTable<string, bool[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(bool[]))]
    public sealed class SaveTableSaveKeyBoolArray : SaveTable<SaveKey, bool[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(byte[]))]
    public sealed class SaveTableIntByteArray : SaveTable<int, byte[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(byte[]))]
    public sealed class SaveTableLongByteArray : SaveTable<long, byte[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(byte[]))]
    public sealed class SaveTableStringByteArray : SaveTable<string, byte[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(byte[]))]
    public sealed class SaveTableSaveKeyByteArray : SaveTable<SaveKey, byte[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(sbyte[]))]
    public sealed class SaveTableIntSbyteArray : SaveTable<int, sbyte[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(sbyte[]))]
    public sealed class SaveTableLongSbyteArray : SaveTable<long, sbyte[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(sbyte[]))]
    public sealed class SaveTableStringSbyteArray : SaveTable<string, sbyte[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(sbyte[]))]
    public sealed class SaveTableSaveKeySbyteArray : SaveTable<SaveKey, sbyte[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(short[]))]
    public sealed class SaveTableIntShortArray : SaveTable<int, short[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(short[]))]
    public sealed class SaveTableLongShortArray : SaveTable<long, short[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(short[]))]
    public sealed class SaveTableStringShortArray : SaveTable<string, short[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(short[]))]
    public sealed class SaveTableSaveKeyShortArray : SaveTable<SaveKey, short[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(ushort[]))]
    public sealed class SaveTableIntUshortArray : SaveTable<int, ushort[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(ushort[]))]
    public sealed class SaveTableLongUshortArray : SaveTable<long, ushort[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(ushort[]))]
    public sealed class SaveTableStringUshortArray : SaveTable<string, ushort[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(ushort[]))]
    public sealed class SaveTableSaveKeyUshortArray : SaveTable<SaveKey, ushort[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(int[]))]
    public sealed class SaveTableIntIntArray : SaveTable<int, int[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(int[]))]
    public sealed class SaveTableLongIntArray : SaveTable<long, int[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(int[]))]
    public sealed class SaveTableStringIntArray : SaveTable<string, int[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(int[]))]
    public sealed class SaveTableSaveKeyIntArray : SaveTable<SaveKey, int[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(uint[]))]
    public sealed class SaveTableIntUintArray : SaveTable<int, uint[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(uint[]))]
    public sealed class SaveTableLongUintArray : SaveTable<long, uint[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(uint[]))]
    public sealed class SaveTableStringUintArray : SaveTable<string, uint[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(uint[]))]
    public sealed class SaveTableSaveKeyUintArray : SaveTable<SaveKey, uint[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(long[]))]
    public sealed class SaveTableIntLongArray : SaveTable<int, long[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(long[]))]
    public sealed class SaveTableLongLongArray : SaveTable<long, long[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(long[]))]
    public sealed class SaveTableStringLongArray : SaveTable<string, long[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(long[]))]
    public sealed class SaveTableSaveKeyLongArray : SaveTable<SaveKey, long[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(ulong[]))]
    public sealed class SaveTableIntUlongArray : SaveTable<int, ulong[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(ulong[]))]
    public sealed class SaveTableLongUlongArray : SaveTable<long, ulong[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(ulong[]))]
    public sealed class SaveTableStringUlongArray : SaveTable<string, ulong[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(ulong[]))]
    public sealed class SaveTableSaveKeyUlongArray : SaveTable<SaveKey, ulong[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(float[]))]
    public sealed class SaveTableIntFloatArray : SaveTable<int, float[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(float[]))]
    public sealed class SaveTableLongFloatArray : SaveTable<long, float[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(float[]))]
    public sealed class SaveTableStringFloatArray : SaveTable<string, float[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(float[]))]
    public sealed class SaveTableSaveKeyFloatArray : SaveTable<SaveKey, float[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(double[]))]
    public sealed class SaveTableIntDoubleArray : SaveTable<int, double[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(double[]))]
    public sealed class SaveTableLongDoubleArray : SaveTable<long, double[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(double[]))]
    public sealed class SaveTableStringDoubleArray : SaveTable<string, double[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(double[]))]
    public sealed class SaveTableSaveKeyDoubleArray : SaveTable<SaveKey, double[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(char[]))]
    public sealed class SaveTableIntCharArray : SaveTable<int, char[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(char[]))]
    public sealed class SaveTableLongCharArray : SaveTable<long, char[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(char[]))]
    public sealed class SaveTableStringCharArray : SaveTable<string, char[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(char[]))]
    public sealed class SaveTableSaveKeyCharArray : SaveTable<SaveKey, char[]> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(string[]))]
    public sealed class SaveTableIntStringArray : SaveTable<int, string[]> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(string[]))]
    public sealed class SaveTableLongStringArray : SaveTable<long, string[]> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(string[]))]
    public sealed class SaveTableStringStringArray : SaveTable<string, string[]> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(string[]))]
    public sealed class SaveTableSaveKeyStringArray : SaveTable<SaveKey, string[]> {}

}
