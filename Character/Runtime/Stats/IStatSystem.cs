using System;
using System.Collections.Generic;
using MisterGames.Actors.Actions;

namespace MisterGames.Character.Stats {
    
    public interface IStatSystem {
        
        event Action OnUpdateModifiers;
        
        float ModifyValue(int statType, float value);
        int ModifyValue(int statType, int value);

        void ForceNotifyUpdate();
        
        void AddModifier(object source, IStatModifier modifier, IActorCondition condition = null);
        void AddModifiers(object source, IReadOnlyList<IStatModifier> modifiers, IActorCondition condition = null);

        void RemoveModifier(object source, IStatModifier modifier);
        void RemoveModifiers(object source, IReadOnlyList<IStatModifier> modifiers);
        void RemoveModifiersOf(object source);
        
        void Clear();
    }
    
}