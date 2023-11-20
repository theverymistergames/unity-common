using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace MisterGames.Common.Data {

    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public class Map<TKey,TValue>:
        IDictionary<TKey,TValue>,
        IDictionary,
        IReadOnlyDictionary<TKey, TValue>

#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif

    {
        [SerializeField] private int[] _buckets;
        [SerializeField] private Entry[] _entries;
        [SerializeField] private int _count;
        [SerializeField] private int _version;
        [SerializeField] private int _freeList;
        [SerializeField] private int _freeCount;

        [Serializable]
        private struct Entry {
            public int hashCode;    // Lower 31 bits of hash code, -1 if unused
            public int next;        // Index of next entry, -1 if last
            public TKey key;        // Key of entry
            public TValue value;    // Value of entry
        }

        public IEqualityComparer<TKey> Comparer => _comparer;
        public int Count => _count - _freeCount;

        public KeyCollection Keys => _keys ??= new KeyCollection(this);
        ICollection IDictionary.Keys => Keys;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _keys ??= new KeyCollection(this);
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _keys ??= new KeyCollection(this);

        public ValueCollection Values => _values ??= new ValueCollection(this);
        ICollection IDictionary.Values => Values;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => _values ??= new ValueCollection(this);
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _values ??= new ValueCollection(this);

        public TValue this[TKey key] {
            get => Get(key);
            set => Insert(key, value, false);
        }

        object IDictionary.this[object key] {
            get {
                if (IsCompatibleKey(key)) {
                    int i = FindEntry((TKey)key);
                    if (i >= 0) {
                        return _entries[i].value;
                    }
                }
                return null;
            }
            set {
                try {
                    var tempKey = (TKey)key;
                    try {
                        this[tempKey] = (TValue)value;
                    }
                    catch (InvalidCastException) {
                        throw new ArgumentException($"Wrong value type {typeof(TValue)}");
                    }
                }
                catch (InvalidCastException) {
                    throw new ArgumentException($"Wrong key type {typeof(TKey)}");
                }
            }
        }

        bool ICollection<KeyValuePair<TKey,TValue>>.IsReadOnly => false;
        bool IDictionary.IsFixedSize => false;
        bool IDictionary.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot {
            get {
                if (_syncRoot == null) {
                    System.Threading.Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        private IEqualityComparer<TKey> _comparer;
        private KeyCollection _keys;
        private ValueCollection _values;
        private object _syncRoot;
        private bool _allowTrimExcess;

        public Map(): this(0, null) {}

        public Map(int capacity): this(capacity, null) {}

        public Map(IEqualityComparer<TKey> comparer): this(0, comparer) {}

        public Map(int capacity, IEqualityComparer<TKey> comparer) {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (capacity > 0) Initialize(capacity);

            _comparer = comparer ?? EqualityComparer<TKey>.Default;
            _allowTrimExcess = true;
        }

        public Map(IDictionary<TKey,TValue> dictionary): this(dictionary, null) {}

        public Map(IDictionary<TKey,TValue> dictionary, IEqualityComparer<TKey> comparer):
            this(dictionary?.Count ?? 0, comparer)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            foreach (var pair in dictionary) {
                Add(pair.Key, pair.Value);
            }
        }

#if UNITY_EDITOR
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() {
            if (_allowTrimExcess) TrimExcess();
        }
#endif

        public void AllowTrimExcess(bool isAllowed) {
            _allowTrimExcess = isAllowed;
        }

        public ref TValue Get(TKey key) {
            int i = FindEntry(key);
            if (i >= 0) return ref _entries[i].value;

            throw new KeyNotFoundException();
        }

        public void Add(TKey key, TValue value) {
            Insert(key, value, true);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair) {
            int i = FindEntry(keyValuePair.Key);
            if (i >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[i].value, keyValuePair.Value)) {
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair) {
            int i = FindEntry(keyValuePair.Key);
            if (i >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[i].value, keyValuePair.Value)) {
                Remove(keyValuePair.Key);
                return true;
            }
            return false;
        }

        public void Clear() {
            if (_count > 0) {
                for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;
                Array.Clear(_entries, 0, _count);
                _freeList = -1;
                _count = 0;
                _freeCount = 0;
                _version++;
            }
        }

        public bool ContainsKey(TKey key) {
            return FindEntry(key) >= 0;
        }

        public bool ContainsValue(TValue value) {
            if (value == null) {
                for (int i = 0; i < _count; i++) {
                    if (_entries[i].hashCode >= 0 && _entries[i].value == null) return true;
                }
            }
            else {
                var c = EqualityComparer<TValue>.Default;
                for (int i = 0; i < _count; i++) {
                    if (_entries[i].hashCode >= 0 && c.Equals(_entries[i].value, value)) return true;
                }
            }
            return false;
        }

        public int IndexOf(TKey key) {
            return FindEntry(key);
        }

        private void CopyTo(KeyValuePair<TKey,TValue>[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (index < 0 || index > array.Length) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (array.Length - index < Count) {
                throw new ArgumentException(nameof(index));
            }

            int count = _count;
            var entries = _entries;
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0) {
                    array[index++] = new KeyValuePair<TKey,TValue>(entries[i].key, entries[i].value);
                }
            }
        }

        public Enumerator GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        private int FindEntry(TKey key) {
            if (_buckets != null) {
                int hashCode = _comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int i = _buckets[hashCode % _buckets.Length]; i >= 0; i = _entries[i].next) {
                    if (_entries[i].hashCode == hashCode && _comparer.Equals(_entries[i].key, key)) return i;
                }
            }
            return -1;
        }

        private void Initialize(int capacity) {
            int size = HashHelpers.GetPrime(capacity);
            _buckets = new int[size];
            for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;
            _entries = new Entry[size];
            _freeList = -1;
        }

        private void Insert(TKey key, TValue value, bool add) {
            if (_buckets is not { Length: > 0 }) Initialize(0);
            int hashCode = _comparer.GetHashCode(key) & 0x7FFFFFFF;
            int targetBucket = hashCode % _buckets.Length;

            for (int i = _buckets[targetBucket]; i >= 0; i = _entries[i].next) {
                if (_entries[i].hashCode == hashCode && _comparer.Equals(_entries[i].key, key)) {
                    if (add) {
                        throw new ArgumentException("Adding duplicate");
                    }
                    _entries[i].value = value;
                    _version++;
                    return;
                }
            }
            int index;
            if (_freeCount > 0) {
                index = _freeList;
                _freeList = _entries[index].next;
                _freeCount--;
            }
            else {
                if (_count == _entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % _buckets.Length;
                }
                index = _count;
                _count++;
            }

            _entries[index].hashCode = hashCode;
            _entries[index].next = _buckets[targetBucket];
            _entries[index].key = key;
            _entries[index].value = value;
            _buckets[targetBucket] = index;
            _version++;
        }

        private void Resize() {
            int newSize = HashHelpers.ExpandPrime(_count);
            Contract.Assert(newSize >= _entries.Length);

            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;

            var newEntries = new Entry[newSize];
            Array.Copy(_entries, 0, newEntries, 0, _count);

            for (int i = 0; i < _count; i++) {
                if (newEntries[i].hashCode >= 0) {
                    int bucket = newEntries[i].hashCode % newSize;
                    newEntries[i].next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }

            _buckets = newBuckets;
            _entries = newEntries;
        }

        private void TrimExcess() {
            int newSize = _count - _freeCount;
            if (newSize == _entries.Length) return;

            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;

            var newEntries = new Entry[newSize];
            int newIndex = 0;

            for (int i = 0; i < _count; i++) {
                ref var entry = ref _entries[i];
                if (entry.hashCode < 0) continue;

                ref var newEntry = ref newEntries[newIndex];

                newEntry.hashCode = entry.hashCode;
                newEntry.key = entry.key;
                newEntry.value = entry.value;

                int bucket = newEntry.hashCode % newSize;
                newEntry.next = newBuckets[bucket];
                newBuckets[bucket] = newIndex++;
            }

            _buckets = newBuckets;
            _entries = newEntries;
            _count = newSize;
            _freeCount = 0;
            _freeList = -1;
        }

        public bool Remove(TKey key) {
            if (_buckets != null) {
                int hashCode = _comparer.GetHashCode(key) & 0x7FFFFFFF;
                int bucket = hashCode % _buckets.Length;
                int last = -1;
                for (int i = _buckets[bucket]; i >= 0; last = i, i = _entries[i].next) {
                    if (_entries[i].hashCode == hashCode && _comparer.Equals(_entries[i].key, key)) {
                        if (last < 0) {
                            _buckets[bucket] = _entries[i].next;
                        }
                        else {
                            _entries[last].next = _entries[i].next;
                        }
                        _entries[i].hashCode = -1;
                        _entries[i].next = _freeList;
                        _entries[i].key = default(TKey);
                        _entries[i].value = default(TValue);
                        _freeList = i;
                        _freeCount++;
                        _version++;
                        return true;
                    }
                }
                if (_allowTrimExcess && _freeCount >= _count) TrimExcess();
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            int i = FindEntry(key);
            if (i >= 0) {
                value = _entries[i].value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue GetValueOrDefault(TKey key) {
            int i = FindEntry(key);
            if (i >= 0) return _entries[i].value;
            return default(TValue);
        }

        void ICollection<KeyValuePair<TKey,TValue>>.CopyTo(KeyValuePair<TKey,TValue>[] array, int index) {
            CopyTo(array, index);
        }

        void ICollection.CopyTo(Array array, int index) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Rank != 1) {
                throw new ArgumentException("Multi-dimensional array is not supported");
            }

            if (array.GetLowerBound(0) != 0 ) {
                throw new ArgumentException("Lower bound of array is non zero");
            }

            if (index < 0 || index > array.Length) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (array.Length - index < Count) {
                throw new ArgumentException(nameof(index));
            }

            if (array is KeyValuePair<TKey, TValue>[] pairs) {
                CopyTo(pairs, index);
            }
            else if (array is DictionaryEntry[] dictEntryArray) {
                var entries = _entries;
                for (int i = 0; i < _count; i++) {
                    if (entries[i].hashCode >= 0) {
                        dictEntryArray[index++] = new DictionaryEntry(entries[i].key, entries[i].value);
                    }
                }
            }
            else {
                if (array is not object[] objects) {
                    throw new ArgumentException("Invalid array type");
                }

                try {
                    int count = _count;
                    var entries = _entries;
                    for (int i = 0; i < count; i++) {
                        if (entries[i].hashCode >= 0) {
                            objects[index++] = new KeyValuePair<TKey,TValue>(entries[i].key, entries[i].value);
                        }
                    }
                }
                catch (ArrayTypeMismatchException) {
                    throw new ArgumentException("Invalid array type");
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        private static bool IsCompatibleKey(object key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            return key is TKey;
        }

        void IDictionary.Add(object key, object value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            try {
                var tempKey = (TKey)key;

                try {
                    Add(tempKey, (TValue)value);
                }
                catch (InvalidCastException) {
                    throw new ArgumentException($"Wrong value type {typeof(TValue)}");
                }
            }
            catch (InvalidCastException) {
                throw new ArgumentException($"Wrong key type {typeof(TKey)}");
            }
        }

        bool IDictionary.Contains(object key) {
            if (IsCompatibleKey(key)) {
                return ContainsKey((TKey)key);
            }

            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new Enumerator(this, Enumerator.DictEntry);
        }

        void IDictionary.Remove(object key) {
            if (IsCompatibleKey(key)) {
                Remove((TKey)key);
            }
        }

        [Serializable]
        public struct Enumerator: IEnumerator<KeyValuePair<TKey,TValue>>, IDictionaryEnumerator {

            public KeyValuePair<TKey,TValue> Current => _current;

            object IEnumerator.Current {
                get {
                    if (_index == 0 || _index == _map._count + 1) {
                        throw new InvalidOperationException("Enumerator current index is invalid");
                    }

                    if (_getEnumeratorRetType == DictEntry) {
                        return new DictionaryEntry(_current.Key, _current.Value);
                    }

                    return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
                }
            }

            DictionaryEntry IDictionaryEnumerator.Entry {
                get {
                    if (_index == 0 || _index == _map._count + 1) {
                        throw new InvalidOperationException("Enumerator current index is invalid");
                    }

                    return new DictionaryEntry(_current.Key, _current.Value);
                }
            }

            object IDictionaryEnumerator.Key {
                get {
                    if (_index == 0 || _index == _map._count + 1) {
                        throw new InvalidOperationException("Enumerator current index is invalid");
                    }

                    return _current.Key;
                }
            }

            object IDictionaryEnumerator.Value {
                get {
                    if (_index == 0 || _index == _map._count + 1) {
                        throw new InvalidOperationException("Enumerator current index is invalid");
                    }

                    return _current.Value;
                }
            }

            private Map<TKey,TValue> _map;
            private int _version;
            private int _index;
            private KeyValuePair<TKey,TValue> _current;
            private int _getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(Map<TKey, TValue> map, int getEnumeratorRetType) {
                _map = map;
                _version = map._version;
                _index = 0;
                _getEnumeratorRetType = getEnumeratorRetType;
                _current = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext() {
                if (_version != _map._version) {
                    throw new InvalidOperationException("Enumerator version is invalid");
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)_index < (uint)_map._count) {
                    if (_map._entries[_index].hashCode >= 0) {
                        _current = new KeyValuePair<TKey, TValue>(_map._entries[_index].key, _map._entries[_index].value);
                        _index++;
                        return true;
                    }
                    _index++;
                }

                _index = _map._count + 1;
                _current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public void Dispose() { }

            void IEnumerator.Reset() {
                if (_version != _map._version) {
                    throw new InvalidOperationException("Enumerator version is invalid");
                }

                _index = 0;
                _current = new KeyValuePair<TKey, TValue>();
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class KeyCollection: ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
        {
            public int Count => _map.Count;
            bool ICollection<TKey>.IsReadOnly => true;
            bool ICollection.IsSynchronized => false;
            object ICollection.SyncRoot => ((ICollection)_map).SyncRoot;

            private readonly Map<TKey,TValue> _map;

            public KeyCollection(Map<TKey,TValue> map) {
                _map = map ?? throw new ArgumentNullException(nameof(map));
            }

            public Enumerator GetEnumerator() {
                return new Enumerator(_map);
            }

            public void CopyTo(TKey[] array, int index) {
                if (index < 0 || index > array.Length) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (array.Length - index < _map.Count) {
                    throw new ArgumentException(nameof(index));
                }

                int count = _map._count;
                var entries = _map._entries;
                for (int i = 0; i < count; i++) {
                    if (entries[i].hashCode >= 0) array[index++] = entries[i].key;
                }
            }

            void ICollection<TKey>.Add(TKey item) {
                throw new NotSupportedException();
            }

            void ICollection<TKey>.Clear() {
                throw new NotSupportedException();
            }

            bool ICollection<TKey>.Contains(TKey item){
                return _map.ContainsKey(item);
            }

            bool ICollection<TKey>.Remove(TKey item) {
                throw new NotSupportedException();
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() {
                return new Enumerator(_map);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new Enumerator(_map);
            }

            void ICollection.CopyTo(Array array, int index) {
                if (array.Rank != 1) {
                    throw new ArgumentException("Multi-dimensional array is not supported");
                }

                if (array.GetLowerBound(0) != 0 ) {
                    throw new ArgumentException("Lower bound of array is non zero");
                }

                if (index < 0 || index > array.Length) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (array.Length - index < _map.Count) {
                    throw new ArgumentException(nameof(index));
                }

                if (array is TKey[] keys) {
                    CopyTo(keys, index);
                }
                else {
                    if (array is not object[] objects) {
                        throw new ArgumentException("Invalid array type");
                    }

                    int count = _map._count;
                    var entries = _map._entries;
                    try {
                        for (int i = 0; i < count; i++) {
                            if (entries[i].hashCode >= 0) objects[index++] = entries[i].key;
                        }
                    }
                    catch (ArrayTypeMismatchException) {
                        throw new ArgumentException("Invalid array type");
                    }
                }
            }

            [Serializable]
            public struct Enumerator : IEnumerator<TKey> {

                public TKey Current => _currentKey;

                object IEnumerator.Current {
                    get {
                        if (_index == 0 || _index == _map._count + 1) {
                            throw new InvalidOperationException("Enumerator current index is invalid");
                        }

                        return _currentKey;
                    }
                }

                private Map<TKey, TValue> _map;
                private int _index;
                private int _version;
                private TKey _currentKey;

                internal Enumerator(Map<TKey, TValue> map) {
                    _map = map;
                    _version = map._version;
                    _index = 0;
                    _currentKey = default(TKey);
                }

                public void Dispose() { }

                public bool MoveNext() {
                    if (_version != _map._version) {
                        throw new InvalidOperationException("Enumerator version is invalid");
                    }

                    while ((uint)_index < (uint)_map._count) {
                        if (_map._entries[_index].hashCode >= 0) {
                            _currentKey = _map._entries[_index].key;
                            _index++;
                            return true;
                        }
                        _index++;
                    }

                    _index = _map._count + 1;
                    _currentKey = default(TKey);
                    return false;
                }

                void IEnumerator.Reset() {
                    if (_version != _map._version) {
                        throw new InvalidOperationException("Enumerator version is invalid");
                    }

                    _index = 0;
                    _currentKey = default(TKey);
                }
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class ValueCollection: ICollection<TValue>, ICollection, IReadOnlyCollection<TValue> {

            public int Count => _map.Count;
            bool ICollection<TValue>.IsReadOnly => true;
            bool ICollection.IsSynchronized => false;
            object ICollection.SyncRoot => ((ICollection)_map).SyncRoot;

            private readonly Map<TKey,TValue> _map;

            public ValueCollection(Map<TKey,TValue> map) {
                _map = map ?? throw new ArgumentNullException(nameof(map));
            }

            public Enumerator GetEnumerator() {
                return new Enumerator(_map);
            }

            public void CopyTo(TValue[] array, int index) {
                if (index < 0 || index > array.Length) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (array.Length - index < _map.Count) {
                    throw new ArgumentException(nameof(index));
                }

                int count = _map._count;
                var entries = _map._entries;
                for (int i = 0; i < count; i++) {
                    if (entries[i].hashCode >= 0) array[index++] = entries[i].value;
                }
            }

            void ICollection<TValue>.Add(TValue item) {
                throw new NotSupportedException();
            }

            bool ICollection<TValue>.Remove(TValue item){
                throw new NotSupportedException();
            }

            void ICollection<TValue>.Clear() {
                throw new NotSupportedException();
            }

            bool ICollection<TValue>.Contains(TValue item){
                return _map.ContainsValue(item);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() {
                return new Enumerator(_map);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new Enumerator(_map);
            }

            void ICollection.CopyTo(Array array, int index) {
                if (array.Rank != 1) {
                    throw new ArgumentException("Multi-dimensional array is not supported");
                }

                if (array.GetLowerBound(0) != 0 ) {
                    throw new ArgumentException("Lower bound of array is non zero");
                }

                if (index < 0 || index > array.Length) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (array.Length - index < _map.Count) {
                    throw new ArgumentException(nameof(index));
                }

                if (array is TValue[] values) {
                    CopyTo(values, index);
                }
                else {
                    if (array is not object[] objects) {
                        throw new ArgumentException("Invalid array type");
                    }

                    int count = _map._count;
                    var entries = _map._entries;
                    try {
                        for (int i = 0; i < count; i++) {
                            if (entries[i].hashCode >= 0) objects[index++] = entries[i].value;
                        }
                    }
                    catch (ArrayTypeMismatchException) {
                        throw new ArgumentException("Invalid array type");
                    }
                }
            }

            [Serializable]
            public struct Enumerator : IEnumerator<TValue> {

                public TValue Current => _currentValue;

                object IEnumerator.Current {
                    get {
                        if (_index == 0 || _index == _map._count + 1) {
                            throw new InvalidOperationException("Enumerator index is invalid");
                        }

                        return _currentValue;
                    }
                }

                private Map<TKey, TValue> _map;
                private int _index;
                private int _version;
                private TValue _currentValue;

                internal Enumerator(Map<TKey, TValue> map) {
                    _map = map;
                    _version = map._version;
                    _index = 0;
                    _currentValue = default(TValue);
                }

                public void Dispose() { }

                public bool MoveNext() {
                    if (_version != _map._version) {
                        throw new InvalidOperationException("Enumerator version is invalid");
                    }

                    while ((uint)_index < (uint)_map._count) {
                        if (_map._entries[_index].hashCode >= 0) {
                            _currentValue = _map._entries[_index].value;
                            _index++;
                            return true;
                        }
                        _index++;
                    }
                    _index = _map._count + 1;
                    _currentValue = default(TValue);
                    return false;
                }

                void IEnumerator.Reset() {
                    if (_version != _map._version) {
                        throw new InvalidOperationException("Enumerator version is invalid");
                    }
                    _index = 0;
                    _currentValue = default(TValue);
                }
            }
        }
    }

}
