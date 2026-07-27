using System;
using MisterGames.Common.Lists;

namespace MisterGames.Input.Bindings {
    
    [Serializable]
    public sealed class KeyBindingListValidator : IBindingValidator {

        public KeyBinding[] allowKeys;
        
        public bool IsMatch(string context) {
            var key = InputBindingExtensions.ToKeyBinding(context);
            return key != KeyBinding.None && allowKeys.Contains(key);
        }
    }
    
}