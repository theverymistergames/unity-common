using System;

namespace MisterGames.UI.Navigation {
    
    [Flags]
    public enum UiNavigationOptions {
        None = 0,
        Scrollable = 1,
        DisallowAnyIncomingNavigation = 2,
        DisallowIncomingNavigationFromOuterNodes = 4,
    }
    
}