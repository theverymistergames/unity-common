using System;

namespace MisterGames.Common.Stats {
    
    [Serializable]
    public struct ValueModifier {
        
        public static ValueModifier Empty = new(ModifierType.Mul, 1f);
        
        public ModifierType operation;
        public float modifier;

        public ValueModifier(ModifierType operation, float modifier) {
            this.operation = operation;
            this.modifier = modifier;
        }

        public float Modify(float value) {
            return operation.Apply(value, modifier);
        }

        public override string ToString() {
            return $"{nameof(ValueModifier)}({operation} {modifier})";
        }
    }
    
}