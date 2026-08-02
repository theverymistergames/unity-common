using System;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    [SaveTable(typeof(int))]
    public sealed class SaveTableIntJson : SaveTableJson<int> {}

    [Serializable]
    [SaveTable(typeof(long))]
    public sealed class SaveTableLongJson : SaveTableJson<long> {}

    [Serializable]
    [SaveTable(typeof(string))]
    public sealed class SaveTableStringJson : SaveTableJson<string> {}

    [Serializable]
    [SaveTable(typeof(SaveKey))]
    public sealed class SaveTableSaveKeyJson : SaveTableJson<SaveKey> {}
    
}