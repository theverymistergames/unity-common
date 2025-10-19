using System;

namespace MisterGames.UI.Navigation {
    
    [Flags]
    public enum UiNavigationMask {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
    }
    
}