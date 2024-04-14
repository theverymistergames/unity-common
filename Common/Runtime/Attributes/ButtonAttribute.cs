using System;

namespace MisterGames.Common.Attributes {
    
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ButtonAttribute : Attribute
    {
        public readonly string name;
        
        public ButtonAttribute(string name = default) {
            this.name = name;
        }
    }
    
}