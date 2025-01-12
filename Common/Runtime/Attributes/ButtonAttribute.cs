using System;

namespace MisterGames.Common.Attributes {
    
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ButtonAttribute : Attribute
    {
        public readonly string name;
        public readonly Mode mode;

        public enum Mode {
            Always,
            Runtime,
            Editor,
        }
        
        public ButtonAttribute(string name = null, Mode mode = Mode.Always) {
            this.name = name;
            this.mode = mode;
        }
    }
    
}