using System;
using MisterGames.Common.Types;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class SerializedTypeMapByRef<TValue> : SerializedDictionaryBase<Type, TValue, SerializedType, TValue> {
        protected override SerializedType SerializeKey(Type key) => new(key);
        protected override Type DeserializeKey(SerializedType key) => key.ToType();
        protected override TValue SerializeValue(TValue value) => value;
        protected override TValue DeserializeValue(TValue value) => value;
    }
    
}
