using System;
using System.Collections;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class SerializedDictionary<TKey, TValue> : SerializedDictionaryBase<TKey, TValue, SerializedDictionary<TKey, TValue>.Entry> {
        [Serializable]
        public struct Entry {
            public TKey key;
            public TValue value;
        }
        protected override Entry Serialize(TKey key, TValue value) => new() { key = key, value = value };
        protected override (TKey, TValue) Deserialize(Entry entry) => (entry.key, entry.value);
    }
    
    [Serializable]
    public sealed class SerializedDictionaryByRef<TKey, TValue> : SerializedDictionaryBase<TKey, TValue, SerializedDictionaryByRef<TKey, TValue>.Entry> {
        [Serializable]
        public struct Entry {
            public TKey key;
            [SerializeReference] [SubclassSelector] public TValue value;
        }
        protected override Entry Serialize(TKey key, TValue value) => new() { key = key, value = value };
        protected override (TKey, TValue) Deserialize(Entry entry) => (entry.key, entry.value);
    }
    
    /// <summary>
    /// Serializable dictionary. TEntry should have "key" and "value" fields to be drawn properly in the inspector.  
    /// </summary>
    [Serializable]
    public abstract class SerializedDictionaryBase<TKey, TValue, TEntry> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver {

        [SerializeField] private List<TEntry> _entries = new();
        [SerializeField] private int _newEntry;

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

        public int FirstIndexOf(Func<TEntry, bool> predicate) {
            for (int i = 0; i < _entries.Count; i++) {
                if (predicate.Invoke(_entries[i])) return i;
            }
            
            return -1;
        }
        
        public int FirstIndexOf<T>(T data, Func<T, TEntry, bool> predicate) {
            for (int i = 0; i < _entries.Count; i++) {
                if (predicate.Invoke(data, _entries[i])) return i;
            }
            
            return -1;
        }
        
        public int LastIndexOf(Func<TEntry, bool> predicate) {
            for (int i = _entries.Count - 1; i >= 0; i--) {
                if (predicate.Invoke(_entries[i])) return i;
            }
            
            return -1;
        }
        
        public int LastIndexOf<T>(T data, Func<T, TEntry, bool> predicate) {
            for (int i = _entries.Count - 1; i >= 0; i--) {
                if (predicate.Invoke(data, _entries[i])) return i;
            }
            
            return -1;
        }
        
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();

        protected abstract TEntry Serialize(TKey key, TValue value);
        protected abstract (TKey, TValue) Deserialize(TEntry entry);
        
        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            TEntry entry = default;
            _newEntry = (_entries?.Count ?? 0) == 0 ? 0 : _newEntry;
            
            if (_newEntry > 0) {
                entry = _entries![^1];
            }
            
            _entries!.Clear();
            foreach (var kvp in _dict) {
                _entries.Add(Serialize(kvp.Key, kvp.Value));
            }

            if (_newEntry > 0) {
                _entries.Add(entry);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            _dict.Clear();

            int count = (_entries?.Count ?? 0) - _newEntry;
            bool isValueType = typeof(TKey).IsValueType;
            
            for (int i = 0; i < count; i++) {
                var entry = _entries![i];
                var (key, value) = Deserialize(entry);
                
                if (!isValueType && key == null) continue;
                
                _dict[key] = value;
            }
        }
    }
    
}
