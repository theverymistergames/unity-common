using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
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
            for (int i = 0; i < _entries.Count; i++) {
                var entry = _entries[i];
                Add(entry.key, entry.value);
            }

            _entries.Clear();
        }

        public SerializedDictionary() : base() {
#if UNITY_EDITOR
            InitUndoRedo();
#endif
        }

        public SerializedDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) {
#if UNITY_EDITOR
            InitUndoRedo();
#endif
        }

        public SerializedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(comparer){
#if UNITY_EDITOR
            InitUndoRedo();
#endif
        }

        public SerializedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) {
#if UNITY_EDITOR
            InitUndoRedo();
#endif
        }

        public SerializedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer) {
#if UNITY_EDITOR
            InitUndoRedo();
#endif
        }

        public SerializedDictionary(IEqualityComparer<TKey> comparer) : base(comparer) {
#if UNITY_EDITOR
            InitUndoRedo();
#endif
        }

        public SerializedDictionary(int capacity) : base(capacity) {
#if UNITY_EDITOR
            InitUndoRedo();
#endif
        }

        public SerializedDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) {
#if UNITY_EDITOR
            InitUndoRedo();
#endif
        }

        protected SerializedDictionary(SerializationInfo info, StreamingContext context) : base(info, context) {
#if UNITY_EDITOR
            InitUndoRedo();
#endif
        }

#if UNITY_EDITOR
        private void InitUndoRedo() {
            Debug.Log("SerializedDictionary.InitUndoRedo");

            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo() {
            Clear();
            for (int i = 0; i < _entries.Count; i++) {
                var entry = _entries[i];
                Add(entry.key, entry.value);
            }
        }
#endif
    }

}
