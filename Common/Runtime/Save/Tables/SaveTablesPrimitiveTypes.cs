using System;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(bool))]
    public sealed class SaveTableBool : SaveTable<bool> {}

    [Serializable]
    [SaveTable(typeof(byte))]
    public sealed class SaveTableByte : SaveTable<byte> {}

    [Serializable]
    [SaveTable(typeof(sbyte))]
    public sealed class SaveTableSbyte : SaveTable<sbyte> {}

    [Serializable]
    [SaveTable(typeof(short))]
    public sealed class SaveTableShort : SaveTable<short> {}

    [Serializable]
    [SaveTable(typeof(ushort))]
    public sealed class SaveTableUshort : SaveTable<ushort> {}

    [Serializable]
    [SaveTable(typeof(int))]
    public sealed class SaveTableInt : SaveTable<int> {}

    [Serializable]
    [SaveTable(typeof(uint))]
    public sealed class SaveTableUint : SaveTable<uint> {}

    [Serializable]
    [SaveTable(typeof(long))]
    public sealed class SaveTableLong : SaveTable<long> {}

    [Serializable]
    [SaveTable(typeof(ulong))]
    public sealed class SaveTableUlong : SaveTable<ulong> {}

    [Serializable]
    [SaveTable(typeof(float))]
    public sealed class SaveTableFloat : SaveTable<float> {}

    [Serializable]
    [SaveTable(typeof(double))]
    public sealed class SaveTableDouble : SaveTable<double> {}

    [Serializable]
    [SaveTable(typeof(char))]
    public sealed class SaveTableChar : SaveTable<char> {}

    [Serializable]
    [SaveTable(typeof(string))]
    public sealed class SaveTableString : SaveTable<string> {}

}
