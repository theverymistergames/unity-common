using System;

namespace MisterGames.UI.Windows {
    
    [Flags]
    public enum UiWindowOptions {
        None = 0,
        HideCursor = 1,
        IgnoreNavigation = 2,
    }
    
}