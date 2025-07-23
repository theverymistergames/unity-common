namespace MisterGames.Common.Stats {
    
    public interface IStatModifier {
        
        /// <summary>
        /// Priority of the modifier in the modifiers sorted list in <see cref="StatSystem"/>.
        /// Modifiers with lower value of the priority are applied first.
        /// It is the first criteria to sort, after that modifiers are sorted by <see cref="Order"/>.
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Order of the modifier in the modifiers sorted list in <see cref="StatSystem"/>.
        /// Modifiers with lower values of the order are applied first.
        /// It is the second criteria to sort, before that modifiers are sorted by <see cref="Priority"/>.
        /// </summary>
        int Order { get; }
        
        /// <summary>
        /// Group of the modifier, which is used to change modifiers behaviour
        /// if applied with some <see cref="ModifierOptions"/>.
        /// </summary>
        int Group { get; }
        
        /// <summary>
        /// Duration in seconds of the modifier effect. If duration is 0, effect is infinite.
        /// </summary>
        float Duration { get; }
        
        /// <summary>
        /// Which stat is this modifier applied to?
        /// </summary>
        int StatType { get; }
        
        /// <summary>
        /// Options to change how the modifier is applied. 
        /// </summary>
        ModifierOptions Options { get; }
        
        /// <summary>
        /// Apply the modifier to the value.
        /// </summary>
        float Modify(float value);
    }
    
}