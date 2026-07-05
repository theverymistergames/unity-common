using System;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(int), typeof(bool))]
    public sealed class SaveTableIntBool : SaveTable<int, bool> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(bool))]
    public sealed class SaveTableLongBool : SaveTable<long, bool> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(bool))]
    public sealed class SaveTableStringBool : SaveTable<string, bool> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(bool))]
    public sealed class SaveTableSaveKeyBool : SaveTable<SaveKey, bool> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(byte))]
    public sealed class SaveTableIntByte : SaveTable<int, byte> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(byte))]
    public sealed class SaveTableLongByte : SaveTable<long, byte> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(byte))]
    public sealed class SaveTableStringByte : SaveTable<string, byte> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(byte))]
    public sealed class SaveTableSaveKeyByte : SaveTable<SaveKey, byte> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(sbyte))]
    public sealed class SaveTableIntSbyte : SaveTable<int, sbyte> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(sbyte))]
    public sealed class SaveTableLongSbyte : SaveTable<long, sbyte> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(sbyte))]
    public sealed class SaveTableStringSbyte : SaveTable<string, sbyte> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(sbyte))]
    public sealed class SaveTableSaveKeySbyte : SaveTable<SaveKey, sbyte> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(short))]
    public sealed class SaveTableIntShort : SaveTable<int, short> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(short))]
    public sealed class SaveTableLongShort : SaveTable<long, short> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(short))]
    public sealed class SaveTableStringShort : SaveTable<string, short> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(short))]
    public sealed class SaveTableSaveKeyShort : SaveTable<SaveKey, short> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(ushort))]
    public sealed class SaveTableIntUshort : SaveTable<int, ushort> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(ushort))]
    public sealed class SaveTableLongUshort : SaveTable<long, ushort> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(ushort))]
    public sealed class SaveTableStringUshort : SaveTable<string, ushort> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(ushort))]
    public sealed class SaveTableSaveKeyUshort : SaveTable<SaveKey, ushort> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(int))]
    public sealed class SaveTableIntInt : SaveTable<int, int> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(int))]
    public sealed class SaveTableLongInt : SaveTable<long, int> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(int))]
    public sealed class SaveTableStringInt : SaveTable<string, int> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(int))]
    public sealed class SaveTableSaveKeyInt : SaveTable<SaveKey, int> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(uint))]
    public sealed class SaveTableIntUint : SaveTable<int, uint> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(uint))]
    public sealed class SaveTableLongUint : SaveTable<long, uint> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(uint))]
    public sealed class SaveTableStringUint : SaveTable<string, uint> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(uint))]
    public sealed class SaveTableSaveKeyUint : SaveTable<SaveKey, uint> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(long))]
    public sealed class SaveTableIntLong : SaveTable<int, long> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(long))]
    public sealed class SaveTableLongLong : SaveTable<long, long> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(long))]
    public sealed class SaveTableStringLong : SaveTable<string, long> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(long))]
    public sealed class SaveTableSaveKeyLong : SaveTable<SaveKey, long> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(ulong))]
    public sealed class SaveTableIntUlong : SaveTable<int, ulong> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(ulong))]
    public sealed class SaveTableLongUlong : SaveTable<long, ulong> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(ulong))]
    public sealed class SaveTableStringUlong : SaveTable<string, ulong> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(ulong))]
    public sealed class SaveTableSaveKeyUlong : SaveTable<SaveKey, ulong> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(float))]
    public sealed class SaveTableIntFloat : SaveTable<int, float> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(float))]
    public sealed class SaveTableLongFloat : SaveTable<long, float> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(float))]
    public sealed class SaveTableStringFloat : SaveTable<string, float> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(float))]
    public sealed class SaveTableSaveKeyFloat : SaveTable<SaveKey, float> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(double))]
    public sealed class SaveTableIntDouble : SaveTable<int, double> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(double))]
    public sealed class SaveTableLongDouble : SaveTable<long, double> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(double))]
    public sealed class SaveTableStringDouble : SaveTable<string, double> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(double))]
    public sealed class SaveTableSaveKeyDouble : SaveTable<SaveKey, double> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(char))]
    public sealed class SaveTableIntChar : SaveTable<int, char> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(char))]
    public sealed class SaveTableLongChar : SaveTable<long, char> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(char))]
    public sealed class SaveTableStringChar : SaveTable<string, char> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(char))]
    public sealed class SaveTableSaveKeyChar : SaveTable<SaveKey, char> {}

    [Serializable]
    [SaveTable(typeof(int), typeof(string))]
    public sealed class SaveTableIntString : SaveTable<int, string> {}

    [Serializable]
    [SaveTable(typeof(long), typeof(string))]
    public sealed class SaveTableLongString : SaveTable<long, string> {}

    [Serializable]
    [SaveTable(typeof(string), typeof(string))]
    public sealed class SaveTableStringString : SaveTable<string, string> {}

    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(string))]
    public sealed class SaveTableSaveKeyString : SaveTable<SaveKey, string> {}

}
