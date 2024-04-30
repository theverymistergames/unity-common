using System;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(bool[]))]
    public sealed class SaveTableBoolArray : SaveTable<bool[]> {}

    [Serializable]
    [SaveTable(typeof(byte[]))]
    public sealed class SaveTableByteArray : SaveTable<byte[]> {}

    [Serializable]
    [SaveTable(typeof(sbyte[]))]
    public sealed class SaveTableSbyteArray : SaveTable<sbyte[]> {}

    [Serializable]
    [SaveTable(typeof(short[]))]
    public sealed class SaveTableShortArray : SaveTable<short[]> {}

    [Serializable]
    [SaveTable(typeof(ushort[]))]
    public sealed class SaveTableUshortArray : SaveTable<ushort[]> {}

    [Serializable]
    [SaveTable(typeof(int[]))]
    public sealed class SaveTableIntArray : SaveTable<int[]> {}

    [Serializable]
    [SaveTable(typeof(uint[]))]
    public sealed class SaveTableUintArray : SaveTable<uint[]> {}

    [Serializable]
    [SaveTable(typeof(long[]))]
    public sealed class SaveTableLongArray : SaveTable<long[]> {}

    [Serializable]
    [SaveTable(typeof(ulong[]))]
    public sealed class SaveTableUlongArray : SaveTable<ulong[]> {}

    [Serializable]
    [SaveTable(typeof(float[]))]
    public sealed class SaveTableFloatArray : SaveTable<float[]> {}

    [Serializable]
    [SaveTable(typeof(double[]))]
    public sealed class SaveTableDoubleArray : SaveTable<double[]> {}

    [Serializable]
    [SaveTable(typeof(char[]))]
    public sealed class SaveTableCharArray : SaveTable<char[]> {}

    [Serializable]
    [SaveTable(typeof(string[]))]
    public sealed class SaveTableStringArray : SaveTable<string[]> {}

}
