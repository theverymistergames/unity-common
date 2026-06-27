using System;
using UnityEngine;

namespace MisterGames.Common.Data {
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TextAreaExtendedAttribute : PropertyAttribute {
        
        public readonly bool expandable;
        public readonly bool showEditButtons;

        public TextAreaExtendedAttribute(bool expandable = true, bool showEditButtons = true) {
            this.expandable = expandable;
            this.showEditButtons = showEditButtons;
        }
    }
    
}