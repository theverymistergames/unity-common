using System;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    [SaveTable(typeof(int), typeof(Enum))]
    public sealed class SaveTableIntEnum : SaveTableEnum<int> { }
    
    [Serializable]
    [SaveTable(typeof(long), typeof(Enum))]
    public sealed class SaveTableLongEnum : SaveTableEnum<long> { }
    
    [Serializable]
    [SaveTable(typeof(string), typeof(Enum))]
    public sealed class SaveTableStringEnum : SaveTableEnum<string> { }
    
    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Enum))]
    public sealed class SaveTableSaveKeyEnum : SaveTableEnum<SaveKey> { }
    
}