using System;
using MisterGames.Common.Lists;

namespace MisterGames.Input.Bindings {
    
    [Serializable]
    public sealed class KeyBindingRangeValidator : IBindingValidator {

        public KeyBinding from;
        public KeyBinding to;
        public KeyBinding[] include;
        public KeyBinding[] exclude;
        
        public bool IsMatch(string context) {
            var key = InputBindingExtensions.ToKeyBinding(context);
            if (key == KeyBinding.None || exclude.Contains(key)) return false;

            int min = Math.Min((int) from, (int) to);
            int max = Math.Max((int) from, (int) to);
            return (int) key >= min && (int) key <= max || include.Contains(key);
        }
    }
    
}