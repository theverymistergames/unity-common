using System;
using System.Collections.Generic;
using MisterGames.Common.Conditions;

namespace MisterGames.Common.Stats {
    
    public interface IStatSystem<out TContext> where TContext : class {
        
        event Action OnUpdateModifiers;
        
        float ModifyValue(int statType, float value);
        int ModifyValue(int statType, int value);

        void ForceNotifyUpdate();
        
        void AddModifier(object source, IStatModifier modifier, ICondition<TContext> condition = null);
        void AddModifiers(object source, IReadOnlyList<IStatModifier> modifiers, ICondition<TContext> condition = null);

        void RemoveModifier(object source, IStatModifier modifier);
        void RemoveModifiers(object source, IReadOnlyList<IStatModifier> modifiers);
        void RemoveModifiersOf(object source);
        
        void Clear();
    }
    
}