using System;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(int), typeof(Object[]))]
    public sealed class SaveTableIntObjectArray : SaveTableArray<int, Object> { }
    
    [Serializable]
    [SaveTable(typeof(long), typeof(Object[]))]
    public sealed class SaveTableLongObjectArray : SaveTableArray<long, Object> { }
    
    [Serializable]
    [SaveTable(typeof(string), typeof(Object[]))]
    public sealed class SaveTableStringObjectArray : SaveTableArray<string, Object> { }
    
    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Object[]))]
    public sealed class SaveTableSaveKeyObjectArray : SaveTableArray<SaveKey, Object> { }

}
