using System;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    [BlackboardTable(typeof(bool))]
    public sealed class BlackboardTableBool : BlackboardTable<bool> {}

    [Serializable]
    [BlackboardTable(typeof(byte))]
    public sealed class BlackboardTableByte : BlackboardTable<byte> {}

    [Serializable]
    [BlackboardTable(typeof(sbyte))]
    public sealed class BlackboardTableSbyte : BlackboardTable<sbyte> {}

    [Serializable]
    [BlackboardTable(typeof(short))]
    public sealed class BlackboardTableShort : BlackboardTable<short> {}

    [Serializable]
    [BlackboardTable(typeof(ushort))]
    public sealed class BlackboardTableUshort : BlackboardTable<ushort> {}

    [Serializable]
    [BlackboardTable(typeof(int))]
    public sealed class BlackboardTableInt : BlackboardTable<int> {}

    [Serializable]
    [BlackboardTable(typeof(uint))]
    public sealed class BlackboardTableUint : BlackboardTable<uint> {}

    [Serializable]
    [BlackboardTable(typeof(long))]
    public sealed class BlackboardTableLong : BlackboardTable<long> {}

    [Serializable]
    [BlackboardTable(typeof(ulong))]
    public sealed class BlackboardTableUlong : BlackboardTable<ulong> {}

    [Serializable]
    [BlackboardTable(typeof(float))]
    public sealed class BlackboardTableFloat : BlackboardTable<float> {}

    [Serializable]
    [BlackboardTable(typeof(double))]
    public sealed class BlackboardTableDouble : BlackboardTable<double> {}

    [Serializable]
    [BlackboardTable(typeof(char))]
    public sealed class BlackboardTableChar : BlackboardTable<char> {}

    [Serializable]
    [BlackboardTable(typeof(string))]
    public sealed class BlackboardTableString : BlackboardTable<string> {}

}
