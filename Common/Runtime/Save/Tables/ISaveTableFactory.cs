using System;
using System.Collections.Generic;

namespace MisterGames.Common.Save.Tables {
    
    public interface ISaveTableFactory {

        IEnumerable<ISaveTable> Tables { get; }
        
        ISaveTable Get<T>();

        ISaveTable Get(Type elementType);
        
        void Set<T>(ISaveTable value);

        void Set(Type elementType, ISaveTable value);

        ISaveTable GetOrCreate<T>();

        ISaveTable GetOrCreate(Type elementType);

        void Prewarm();

        void Clear();
    }
    
}