using System;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    [SaveTable(typeof(int), typeof(object[]))]
    public sealed class SaveTableIntByRefArray : SaveTableByRefArray<int, object> { }
    
    [Serializable]
    [SaveTable(typeof(long), typeof(object[]))]
    public sealed class SaveTableLongByRefArray : SaveTableByRefArray<long, object> { }
    
    [Serializable]
    [SaveTable(typeof(string), typeof(object[]))]
    public sealed class SaveTableStringByRefArray : SaveTableByRefArray<string, object> { }
    
    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(object[]))]
    public sealed class SaveTableSaveKeyByRefArray : SaveTableByRefArray<SaveKey, object> { }
    
}