using System;
using System.Collections.Generic;
using MisterGames.Common.Data;

namespace MisterGames.Actors
{
    
    public readonly struct ComponentCollection<T> where T : class {
        
        private readonly MultiValueDictionary<Type, object>.ValueCollection _valueCollection;
        private readonly List<IActorComponent>.Enumerator _list;
        private readonly bool useList;
        
        public ComponentCollection(MultiValueDictionary<Type, object> source) {
            _valueCollection = source.GetValues(typeof(T));
            _list = default;
            useList = false;
        }

        public ComponentCollection(List<IActorComponent> source) {
            _valueCollection = default;
            _list = source.GetEnumerator();
            useList = true;
        }
        
        public T Current => (useList ? _list.Current : _valueCollection.Current) as T;
        public bool MoveNext() => useList ? _list.MoveNext() : _valueCollection.MoveNext();
        public ComponentCollection<T> GetEnumerator() => this;
    }
    
}