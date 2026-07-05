using System;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(int), typeof(object))]
    public sealed class SaveTableIntReference : SaveTableByRef<int, object> { }
    
    [Serializable]
    [SaveTable(typeof(long), typeof(object))]
    public sealed class SaveTableLongReference : SaveTableByRef<long, object> { }
    
    [Serializable]
    [SaveTable(typeof(string), typeof(object))]
    public sealed class SaveTableStringReference : SaveTableByRef<string, object> { }
    
    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(object))]
    public sealed class SaveTableSaveKeyReference : SaveTableByRef<SaveKey, object> { }
    
}
