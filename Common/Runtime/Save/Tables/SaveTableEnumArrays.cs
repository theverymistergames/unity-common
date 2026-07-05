using System;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    [SaveTable(typeof(int), typeof(Enum[]))]
    public sealed class SaveTableIntEnumArray : SaveTableEnumArray<int> { }
    
    [Serializable]
    [SaveTable(typeof(long), typeof(Enum[]))]
    public sealed class SaveTableLongEnumArray : SaveTableEnumArray<long> { }
    
    [Serializable]
    [SaveTable(typeof(string), typeof(Enum[]))]
    public sealed class SaveTableStringEnumArray : SaveTableEnumArray<string> { }
    
    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Enum[]))]
    public sealed class SaveTableSaveKeyEnumArray : SaveTableEnumArray<SaveKey> { }
    
}