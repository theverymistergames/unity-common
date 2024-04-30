using System;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(object))]
    public sealed class SaveTableReference : SaveTableByRef<object> { }
    
}
