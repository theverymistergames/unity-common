using System;

namespace MisterGames.Common.Stats {
    
    [Serializable]
    public struct ValueModifier {
        
        public static ValueModifier Empty = new ValueModifier(OperationType.Mul, 1f);
        
        public OperationType operation;
        public float modifier;

        public ValueModifier(OperationType operation, float modifier) {
            this.operation = operation;
            this.modifier = modifier;
        }

        public float Modify(float value) {
            return operation.Apply(value, modifier);
        }
    }
    
}