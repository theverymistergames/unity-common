using System;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    [BlackboardTable(typeof(bool[]))]
    public sealed class BlackboardTableBoolArray : BlackboardTable<bool[]> {}

    [Serializable]
    [BlackboardTable(typeof(byte[]))]
    public sealed class BlackboardTableByteArray : BlackboardTable<byte[]> {}

    [Serializable]
    [BlackboardTable(typeof(sbyte[]))]
    public sealed class BlackboardTableSbyteArray : BlackboardTable<sbyte[]> {}

    [Serializable]
    [BlackboardTable(typeof(short[]))]
    public sealed class BlackboardTableShortArray : BlackboardTable<short[]> {}

    [Serializable]
    [BlackboardTable(typeof(ushort[]))]
    public sealed class BlackboardTableUshortArray : BlackboardTable<ushort[]> {}

    [Serializable]
    [BlackboardTable(typeof(int[]))]
    public sealed class BlackboardTableIntArray : BlackboardTable<int[]> {}

    [Serializable]
    [BlackboardTable(typeof(uint[]))]
    public sealed class BlackboardTableUintArray : BlackboardTable<uint[]> {}

    [Serializable]
    [BlackboardTable(typeof(long[]))]
    public sealed class BlackboardTableLongArray : BlackboardTable<long[]> {}

    [Serializable]
    [BlackboardTable(typeof(ulong[]))]
    public sealed class BlackboardTableUlongArray : BlackboardTable<ulong[]> {}

    [Serializable]
    [BlackboardTable(typeof(float[]))]
    public sealed class BlackboardTableFloatArray : BlackboardTable<float[]> {}

    [Serializable]
    [BlackboardTable(typeof(double[]))]
    public sealed class BlackboardTableDoubleArray : BlackboardTable<double[]> {}

    [Serializable]
    [BlackboardTable(typeof(char[]))]
    public sealed class BlackboardTableCharArray : BlackboardTable<char[]> {}

    [Serializable]
    [BlackboardTable(typeof(string[]))]
    public sealed class BlackboardTableStringArray : BlackboardTable<string[]> {}

}
