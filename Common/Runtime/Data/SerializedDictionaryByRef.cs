using System;
using System.Collections;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class SerializedDictionaryByRef<TKey, TValue> : SerializedDictionaryBaseByRef<TKey, TValue, TKey, TValue> {
        protected override TKey SerializeKey(TKey key) => key;
        protected override TKey DeserializeKey(TKey key) => key;
        protected override TValue SerializeValue(TValue value) => value;
        protected override TValue DeserializeValue(TValue value) => value;
    }
    
    [Serializable]
    public abstract class SerializedDictionaryBaseByRef<TKey, TValue, TSerializedKey, TSerializedValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver {

        [SerializeField] private List<Entry> _entries = new();
        [SerializeField] private int _confirmedCount = -1;

        [Serializable]
        internal struct Entry {
            public TSerializedKey key;
            [SerializeReference] [SubclassSelector] public TSerializedValue value;
        }

        private readonly Dictionary<TKey, TValue> _dict = new();

        public TValue this[TKey key] { get => _dict[key]; set => _dict[key] = value; }
        public ICollection<TKey> Keys => _dict.Keys;
        public ICollection<TValue> Values => _dict.Values;
        public int Count => _dict.Count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value) => _dict.Add(key, value);
        public void Add(KeyValuePair<TKey, TValue> item) => _dict.Add(item.Key, item.Value);
        public bool TryAdd(TKey key, TValue value) => _dict.TryAdd(key, value);
        public bool Remove(TKey key) => _dict.Remove(key);
        public bool Remove(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>) _dict).Remove(item);
        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
        public bool ContainsValue(TValue value) => _dict.ContainsValue(value);
        public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>) _dict).Contains(item);
        public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);
        public TValue GetValueOrDefault(TKey key, TValue defaultValue = default) => _dict.GetValueOrDefault(key, defaultValue);
        public void Clear() => _dict.Clear();

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            ((IDictionary<TKey, TValue>) _dict).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if (_confirmedCount >= 0) return;

            _entries.Clear();
            foreach (var kvp in _dict) {
                _entries.Add(new Entry { key = SerializeKey(kvp.Key), value = SerializeValue(kvp.Value) });
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            _dict.Clear();

            int count = _confirmedCount >= 0 ? _confirmedCount : _entries.Count;
            for (int i = 0; i < count; i++) {
                var entry = _entries[i];
                _dict[DeserializeKey(entry.key)] = DeserializeValue(entry.value);
            }
        }

        protected abstract TSerializedKey SerializeKey(TKey key);
        protected abstract TKey DeserializeKey(TSerializedKey key);
        protected abstract TSerializedValue SerializeValue(TValue value);
        protected abstract TValue DeserializeValue(TSerializedValue value);
    }
    
}
