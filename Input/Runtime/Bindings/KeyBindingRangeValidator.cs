using System;

namespace MisterGames.Input.Bindings {
    
    [Serializable]
    public sealed class KeyBindingRangeValidator : IBindingValidator {

        public KeyBinding from;
        public KeyBinding to;
        
        public bool IsMatch(string context) {
            var key = InputBindingExtensions.ToKeyBinding(context);
            if (key == KeyBinding.None) return false;

            int min = Math.Min((int) from, (int) to);
            int max = Math.Max((int) from, (int) to);
            return (int) key >= min && (int) key <= max;
        }
    }
    
}