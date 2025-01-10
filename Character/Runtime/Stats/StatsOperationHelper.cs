using System;

namespace MisterGames.Character.Stats {
    
    public static class StatsOperationHelper {
        
        public static float Apply(this OperationType operationType, float value, float modifier) {
            return operationType switch {
                OperationType.Add => value + modifier,
                OperationType.Multiply => value * modifier,
                OperationType.Min => value < modifier ? modifier : value,
                OperationType.Max => value > modifier ? modifier : value,
                OperationType.Set => modifier,
                _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null),
            };
        }
    }
    
}