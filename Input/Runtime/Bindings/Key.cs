using System;
using MisterGames.Input.Global;

namespace MisterGames.Input.Bindings {
    
    [Serializable]
    public sealed class Key : IKeyBinding {
        
        public KeyBinding key;

        public bool IsActive => key.IsActive();
    }
    
}
