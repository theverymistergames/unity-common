using System;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(int), typeof(Object))]
    public sealed class SaveTableIntObject : SaveTable<int, Object> { }
    
    [Serializable]
    [SaveTable(typeof(long), typeof(Object))]
    public sealed class SaveTableLongObject : SaveTable<long, Object> { }
    
    [Serializable]
    [SaveTable(typeof(string), typeof(Object))]
    public sealed class SaveTableStringObject : SaveTable<string, Object> { }
    
    [Serializable]
    [SaveTable(typeof(SaveKey), typeof(Object))]
    public sealed class SaveTableSaveKeyObject : SaveTable<SaveKey, Object> { }

}
