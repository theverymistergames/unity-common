using System;

namespace MisterGames.Common.Stats {
    
    public static class StatsOperationHelper {
        
        public static float Apply(this ModifierType operationType, float value, float modifier) {
            return operationType switch {
                ModifierType.Add => value + modifier,
                ModifierType.Mul => value * modifier,
                ModifierType.Min => value < modifier ? modifier : value,
                ModifierType.Max => value > modifier ? modifier : value,
                ModifierType.Set => modifier,
                _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null),
            };
        }
    }
    
}