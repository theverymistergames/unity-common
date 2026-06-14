using System;

namespace MisterGames.Common.Attributes {
    
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ButtonAttribute : Attribute
    {
        public readonly string name;
        public readonly Mode mode;
        public readonly string showIf;

        public enum Mode {
            Always,
            Runtime,
            Editor,
        }
        
        public ButtonAttribute(string name = null, Mode mode = Mode.Always, string showIf = null) {
            this.name = name;
            this.mode = mode;
            this.showIf = showIf;
        }
    }
    
}