using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Types;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class SerializedTypeMap<TValue> : SerializedDictionaryBase<Type, TValue, SerializedTypeMap<TValue>.Entry> {
        [Serializable]
        public struct Entry {
            public SerializedType key;
            public TValue value;
        }
        protected override Entry Serialize(Type key, TValue value) => new() { key = new SerializedType(key), value = value };
        protected override (Type, TValue) Deserialize(Entry entry) => (entry.key.ToType(), entry.value);
    }
    
    [Serializable]
    public sealed class SerializedTypeMapByRef<TValue> : SerializedDictionaryBase<Type, TValue, SerializedTypeMapByRef<TValue>.Entry> {
        [Serializable]
        public struct Entry {
            public SerializedType key;
            [SerializeReference] [SubclassSelector] public TValue value;
        }
        protected override Entry Serialize(Type key, TValue value) => new() { key = new SerializedType(key), value = value };
        protected override (Type, TValue) Deserialize(Entry entry) => (entry.key.ToType(), entry.value);
    }
    
}
