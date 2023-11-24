using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace MisterGames.Common.Data {

    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public class Map<K, V> : IDictionary<K, V>, IDictionary, IReadOnlyDictionary<K, V>, ISerializationCallbackReceiver {

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
            public K key;           // Key of entry
            public V value;         // Value of entry
        }

        public IEqualityComparer<K> Comparer => _comparer;
        public int Count => _count - _freeCount;

        public KeyCollection Keys => _keys ??= new KeyCollection(this);
        ICollection IDictionary.Keys => Keys;
        ICollection<K> IDictionary<K, V>.Keys => _keys ??= new KeyCollection(this);
        IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => _keys ??= new KeyCollection(this);

        public ValueCollection Values => _values ??= new ValueCollection(this);
        ICollection IDictionary.Values => Values;
        ICollection<V> IDictionary<K, V>.Values => _values ??= new ValueCollection(this);
        IEnumerable<V> IReadOnlyDictionary<K, V>.Values => _values ??= new ValueCollection(this);

        public V this[K key] {
            get => Get(key);
            set => Insert(key, value, false);
        }

        object IDictionary.this[object key] {
            get {
                if (IsCompatibleKey(key)) {
                    int i = FindEntry((K)key);
                    if (i >= 0) return _entries[i].value;
                }
                return null;
            }
            set {
                try {
                    var tempKey = (K)key;
                    try {
                        this[tempKey] = (V)value;
                    }
                    catch (InvalidCastException) {
                        throw new ArgumentException($"Wrong value type {typeof(V)}");
                    }
                }
                catch (InvalidCastException) {
                    throw new ArgumentException($"Wrong key type {typeof(K)}");
                }
            }
        }

        bool ICollection<KeyValuePair<K,V>>.IsReadOnly => false;
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

        private IEqualityComparer<K> _comparer;
        private KeyCollection _keys;
        private ValueCollection _values;
        private object _syncRoot;
        private bool _isTrimExcessAllowed = true;

        public Map(): this(0, null) {}

        public Map(int capacity): this(capacity, null) {}

        public Map(IEqualityComparer<K> comparer): this(0, comparer) {}

        public Map(int capacity, IEqualityComparer<K> comparer) {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (capacity > 0) Initialize(capacity);

            _comparer = comparer ?? EqualityComparer<K>.Default;
        }

        public Map(IDictionary<K, V> dictionary): this(dictionary, null) {}

        public Map(IDictionary<K, V> dictionary, IEqualityComparer<K> comparer):
            this(dictionary?.Count ?? 0, comparer)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            foreach (var pair in dictionary) {
                Add(pair.Key, pair.Value);
            }
        }

        public Map(Map<K, V> map): this(map, null) {}

        public Map(Map<K, V> map, IEqualityComparer<K> comparer):
            this(map?.Count ?? 0, comparer)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            _buckets = new int[map._buckets.Length];
            Array.Copy(map._buckets, _buckets, map._buckets.Length);

            _entries = new Entry[map._entries.Length];
            Array.Copy(map._entries, _entries, map._entries.Length);

            _count = map._count;
            _freeCount = map._freeCount;
            _freeList = map._freeList;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() {
            TrimExcess(forceNewHashCodes: true);
        }

        public ref V Get(K key) {
            int i = FindEntry(key);
            if (i >= 0) return ref _entries[i].value;

            throw new KeyNotFoundException();
        }

        public void Add(K key, V value) {
            Insert(key, value, true);
        }

        void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> keyValuePair) {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> keyValuePair) {
            int i = FindEntry(keyValuePair.Key);
            if (i >= 0 && EqualityComparer<V>.Default.Equals(_entries[i].value, keyValuePair.Value)) {
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> keyValuePair) {
            int i = FindEntry(keyValuePair.Key);
            if (i >= 0 && EqualityComparer<V>.Default.Equals(_entries[i].value, keyValuePair.Value)) {
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

        public bool ContainsKey(K key) {
            return FindEntry(key) >= 0;
        }

        public bool ContainsValue(V value) {
            if (value == null) {
                for (int i = 0; i < _count; i++) {
                    if (_entries[i].hashCode >= 0 && _entries[i].value == null) return true;
                }
            }
            else {
                var c = EqualityComparer<V>.Default;
                for (int i = 0; i < _count; i++) {
                    if (_entries[i].hashCode >= 0 && c.Equals(_entries[i].value, value)) return true;
                }
            }
            return false;
        }

        public int IndexOf(K key) {
            return FindEntry(key);
        }

        private void CopyTo(KeyValuePair<K,V>[] array, int index) {
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
                    array[index++] = new KeyValuePair<K,V>(entries[i].key, entries[i].value);
                }
            }
        }

        public Enumerator GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        private int FindEntry(K key) {
            if (_buckets is { Length: > 0 }) {
                int hashCode = _comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int i = _buckets[hashCode % _buckets.Length]; i >= 0; i = _entries[i].next) {
                    if (_entries[i].hashCode == hashCode && _comparer.Equals(_entries[i].key, key)) return i;
                }
            }
            return -1;
        }

        private void Initialize(int capacity) {
            int size = HashHelpers.GetPrime(capacity);

#if UNITY_EDITOR
            if (capacity <= 1) size = 1;
            else if (capacity <= 3) size = capacity;
#endif

            _buckets = new int[size];
            for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;
            _entries = new Entry[size];
            _freeList = -1;
        }

        private void Insert(K key, V value, bool add) {
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
                if (_count == _entries.Length) {
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

#if UNITY_EDITOR
            TrimExcess(false);
#endif
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

        private void AllowTrimExcess(bool isAllowed) {
            _isTrimExcessAllowed = isAllowed;
            if (isAllowed) TrimExcess(false);
        }

        private void TrimExcess(bool forceNewHashCodes) {
            if (!_isTrimExcessAllowed) return;

            int newSize = _count - _freeCount;
            if (!forceNewHashCodes && newSize == _entries.Length) return;

            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;

            var newEntries = new Entry[newSize];
            int newIndex = 0;

            for (int i = 0; i < _count; i++) {
                ref var entry = ref _entries[i];
                if (entry.hashCode < 0) continue;

                ref var newEntry = ref newEntries[newIndex];

                newEntry.hashCode = forceNewHashCodes ? _comparer.GetHashCode(entry.key) & 0x7FFFFFFF : entry.hashCode;
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

        public bool RemoveIf<T>(T target, Func<T, K, bool> predicate) {
            AllowTrimExcess(false);
            bool removed = false;

            for (int i = 0; i < _entries.Length; i++) {
                ref var entry = ref _entries[i];
                if (entry.hashCode >= 0 && predicate.Invoke(target, entry.key)) removed = Remove(entry.key);
            }

            AllowTrimExcess(true);
            return removed;
        }

        public bool Remove(K key) {
            if (_buckets is { Length: > 0 }) {
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
                        _entries[i].key = default(K);
                        _entries[i].value = default(V);
                        _freeList = i;
                        _freeCount++;
                        _version++;
                        return true;
                    }
                }
#if UNITY_EDITOR
                TrimExcess(forceNewHashCodes: false);
#else
                if (_freeCount > _count) TrimExcess(forceNewHashCodes: false);
#endif
            }
            return false;
        }

        public bool TryGetValue(K key, out V value) {
            int i = FindEntry(key);
            if (i >= 0) {
                value = _entries[i].value;
                return true;
            }
            value = default(V);
            return false;
        }

        public V GetValueOrDefault(K key) {
            int i = FindEntry(key);
            if (i >= 0) return _entries[i].value;
            return default(V);
        }

        void ICollection<KeyValuePair<K,V>>.CopyTo(KeyValuePair<K,V>[] array, int index) {
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

            if (array is KeyValuePair<K, V>[] pairs) {
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
                            objects[index++] = new KeyValuePair<K,V>(entries[i].key, entries[i].value);
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

            return key is K;
        }

        void IDictionary.Add(object key, object value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            try {
                var tempKey = (K)key;

                try {
                    Add(tempKey, (V)value);
                }
                catch (InvalidCastException) {
                    throw new ArgumentException($"Wrong value type {typeof(V)}");
                }
            }
            catch (InvalidCastException) {
                throw new ArgumentException($"Wrong key type {typeof(K)}");
            }
        }

        bool IDictionary.Contains(object key) {
            if (IsCompatibleKey(key)) {
                return ContainsKey((K)key);
            }

            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new Enumerator(this, Enumerator.DictEntry);
        }

        void IDictionary.Remove(object key) {
            if (IsCompatibleKey(key)) {
                Remove((K)key);
            }
        }

        [Serializable]
        public struct Enumerator: IEnumerator<KeyValuePair<K,V>>, IDictionaryEnumerator {

            public KeyValuePair<K,V> Current => _current;

            object IEnumerator.Current {
                get {
                    if (_index == 0 || _index == _map._count + 1) {
                        throw new InvalidOperationException("Enumerator current index is invalid");
                    }

                    if (_getEnumeratorRetType == DictEntry) {
                        return new DictionaryEntry(_current.Key, _current.Value);
                    }

                    return new KeyValuePair<K, V>(_current.Key, _current.Value);
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

            private Map<K,V> _map;
            private int _version;
            private int _index;
            private KeyValuePair<K,V> _current;
            private int _getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(Map<K, V> map, int getEnumeratorRetType) {
                _map = map;
                _version = map._version;
                _index = 0;
                _getEnumeratorRetType = getEnumeratorRetType;
                _current = new KeyValuePair<K, V>();
            }

            public bool MoveNext() {
                if (_version != _map._version) {
                    throw new InvalidOperationException("Enumerator version is invalid");
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)_index < (uint)_map._count) {
                    if (_map._entries[_index].hashCode >= 0) {
                        _current = new KeyValuePair<K, V>(_map._entries[_index].key, _map._entries[_index].value);
                        _index++;
                        return true;
                    }
                    _index++;
                }

                _index = _map._count + 1;
                _current = new KeyValuePair<K, V>();
                return false;
            }

            public void Dispose() { }

            void IEnumerator.Reset() {
                if (_version != _map._version) {
                    throw new InvalidOperationException("Enumerator version is invalid");
                }

                _index = 0;
                _current = new KeyValuePair<K, V>();
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class KeyCollection: ICollection<K>, ICollection, IReadOnlyCollection<K>
        {
            public int Count => _map.Count;
            bool ICollection<K>.IsReadOnly => true;
            bool ICollection.IsSynchronized => false;
            object ICollection.SyncRoot => ((ICollection)_map).SyncRoot;

            private readonly Map<K,V> _map;

            public KeyCollection(Map<K,V> map) {
                _map = map ?? throw new ArgumentNullException(nameof(map));
            }

            public Enumerator GetEnumerator() {
                return new Enumerator(_map);
            }

            public void CopyTo(K[] array, int index) {
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

            void ICollection<K>.Add(K item) {
                throw new NotSupportedException();
            }

            void ICollection<K>.Clear() {
                throw new NotSupportedException();
            }

            bool ICollection<K>.Contains(K item){
                return _map.ContainsKey(item);
            }

            bool ICollection<K>.Remove(K item) {
                throw new NotSupportedException();
            }

            IEnumerator<K> IEnumerable<K>.GetEnumerator() {
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

                if (array is K[] keys) {
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
            public struct Enumerator : IEnumerator<K> {

                public K Current => _currentKey;

                object IEnumerator.Current {
                    get {
                        if (_index == 0 || _index == _map._count + 1) {
                            throw new InvalidOperationException("Enumerator current index is invalid");
                        }

                        return _currentKey;
                    }
                }

                private Map<K, V> _map;
                private int _index;
                private int _version;
                private K _currentKey;

                internal Enumerator(Map<K, V> map) {
                    _map = map;
                    _version = map._version;
                    _index = 0;
                    _currentKey = default(K);
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
                    _currentKey = default(K);
                    return false;
                }

                void IEnumerator.Reset() {
                    if (_version != _map._version) {
                        throw new InvalidOperationException("Enumerator version is invalid");
                    }

                    _index = 0;
                    _currentKey = default(K);
                }
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class ValueCollection: ICollection<V>, ICollection, IReadOnlyCollection<V> {

            public int Count => _map.Count;
            bool ICollection<V>.IsReadOnly => true;
            bool ICollection.IsSynchronized => false;
            object ICollection.SyncRoot => ((ICollection)_map).SyncRoot;

            private readonly Map<K,V> _map;

            public ValueCollection(Map<K,V> map) {
                _map = map ?? throw new ArgumentNullException(nameof(map));
            }

            public Enumerator GetEnumerator() {
                return new Enumerator(_map);
            }

            public void CopyTo(V[] array, int index) {
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

            void ICollection<V>.Add(V item) {
                throw new NotSupportedException();
            }

            bool ICollection<V>.Remove(V item){
                throw new NotSupportedException();
            }

            void ICollection<V>.Clear() {
                throw new NotSupportedException();
            }

            bool ICollection<V>.Contains(V item){
                return _map.ContainsValue(item);
            }

            IEnumerator<V> IEnumerable<V>.GetEnumerator() {
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

                if (array is V[] values) {
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
            public struct Enumerator : IEnumerator<V> {

                public V Current => _currentValue;

                object IEnumerator.Current {
                    get {
                        if (_index == 0 || _index == _map._count + 1) {
                            throw new InvalidOperationException("Enumerator index is invalid");
                        }

                        return _currentValue;
                    }
                }

                private Map<K, V> _map;
                private int _index;
                private int _version;
                private V _currentValue;

                internal Enumerator(Map<K, V> map) {
                    _map = map;
                    _version = map._version;
                    _index = 0;
                    _currentValue = default(V);
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
                    _currentValue = default(V);
                    return false;
                }

                void IEnumerator.Reset() {
                    if (_version != _map._version) {
                        throw new InvalidOperationException("Enumerator version is invalid");
                    }
                    _index = 0;
                    _currentValue = default(V);
                }
            }
        }
    }

}
