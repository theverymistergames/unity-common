using System;

namespace MisterGames.Common.Stats {
    
    [Flags]
    public enum ModifierOptions {
        None = 0,
        
        /// <summary>
        /// Only last added modifier in the group is applied to the stat.
        /// Modifiers are contained in one group, if they have same
        /// <see cref="IStatModifier.StatType"/>,
        /// <see cref="IStatModifier.Priority"/> and
        /// <see cref="IStatModifier.Group"/> values.
        /// </summary>
        OverwriteLast = 1,
        
        /// <summary>
        /// Modifier is removed after its condition returns false.
        /// </summary>
        RemoveOnConditionFail = 2,
        
        /// <summary>
        /// Modifier is removed after its condition returns false,
        /// if this is the last modifier in the group and has this option.
        /// Modifiers are contained in one group, if they have same
        /// <see cref="IStatModifier.StatType"/>,
        /// <see cref="IStatModifier.Priority"/> and
        /// <see cref="IStatModifier.Group"/> values.
        /// </summary>
        RemoveLastOnConditionFail = 4,
    }
    
}