using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public abstract class SerializedDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver {

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        [Serializable]
        private struct Entry {
            public K key;
            public V value;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            _entries.Clear();

            foreach (var kvp in this) {
                _entries.Add(new Entry { key = kvp.Key, value = kvp.Value });
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            for (int i = 0; i < _entries.Count; i++) {
                var entry = _entries[i];
                Add(entry.key, entry.value);
            }

            _entries.Clear();
        }
    }

}
