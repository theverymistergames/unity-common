using System;

namespace MisterGames.UI.Navigation {
    
    [Flags]
    public enum UiNavigationOuterOptions {
        None = 0,
        Parent = 1,
        Siblings = 2,
        Children = 4,
    }
    
}