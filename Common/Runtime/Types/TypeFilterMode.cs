using System;

namespace MisterGames.Common.Types {
    
    [Flags]
    public enum TypeFilterMode {
        None = 0,
        Classes = 1,
        Interfaces = 2,
        ValueTypes = 4,
        ExcludeSelf = 8,
    }
    
}