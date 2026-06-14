using System;
using System.Collections.Generic;

namespace MisterGames.Actors {
    
    public interface ISetBuilder<T> {
        
        S Get<S>() where S : T;
        T Get(Type type);
        
        ISetBuilder<T> Set(IReadOnlyList<T> list);
        ISetBuilder<T> Set(T data);
        ISetBuilder<T> Set<S>() where S : T;
        ISetBuilder<T> Set(Type type);
        
        ISetBuilder<T> Remove<S>() where S : T;
        ISetBuilder<T> Remove(Type type);
        
        IReadOnlyList<T> GetResultArray();
        void Clear();
    }
    
}