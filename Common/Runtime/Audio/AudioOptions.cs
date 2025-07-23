using System;

namespace MisterGames.Common.Audio {
    
    [Flags]
    public enum AudioOptions {
        None = 0,
        Loop = 1,
        AffectedByTimeScale = 2,
        ApplyOcclusion = 4,
        AffectedByVolumes = 8,
    }
    
}