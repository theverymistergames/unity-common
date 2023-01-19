using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        [Serializable]
        private struct Entry {
            public TKey key;
            public TValue value;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            _entries.Clear();

            foreach (var kvp in this) {
                _entries.Add(new Entry { key = kvp.Key, value = kvp.Value });
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            Clear();

            for (int i = 0; i < _entries.Count; i++) {
                var entry = _entries[i];
                Add(entry.key, entry.value);
            }
        }

#if UNITY_EDITOR
        private void InitUndoRedo() {
            UnityEditor.Undo.undoRedoPerformed -= OnUndoRedo;
            UnityEditor.Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo() {
            Clear();

            for (int i = 0; i < _entries.Count; i++) {
                var entry = _entries[i];
                Add(entry.key, entry.value);
            }
        }

        public SerializedDictionary() : base() {
            InitUndoRedo();
        }

        public SerializedDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) {
            InitUndoRedo();
        }

        public SerializedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer){
            InitUndoRedo();
        }

        public SerializedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) {
            InitUndoRedo();
        }

        public SerializedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer) {
            InitUndoRedo();
        }

        public SerializedDictionary(IEqualityComparer<TKey> comparer) : base(comparer) {
            InitUndoRedo();
        }

        public SerializedDictionary(int capacity) : base(capacity) {
            InitUndoRedo();
        }

        public SerializedDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) {
            InitUndoRedo();
        }

        protected SerializedDictionary(SerializationInfo info, StreamingContext context) : base(info, context) {
            InitUndoRedo();
        }
#endif
    }

}
