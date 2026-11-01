using System;

namespace MisterGames.Common.Inputs {

    [Flags]
    public enum LightOptions {
        None = 0,
        L = 1,
        Lc = 1 << 1,
        C = 1 << 2,
        Rc = 1 << 3,
        R = 1 << 4,
        SmoothFade = 1 << 5,
    }

}
