using System;
using System.Collections.Generic;
using MisterGames.Common.Labels.Base;
namespace MisterGames.Common.Labels {
    
    public sealed class LabelValueRuntimeStorage : ILabelValueRuntimeStorage, IDisposable {

        private readonly struct Key : IEquatable<Key> {
            
            private readonly int _libraryId;
            private readonly int _valueId;
            
            public Key(int libraryId, int valueId) {
                _libraryId = libraryId;
                _valueId = valueId;
            }
            
            public bool Equals(Key other) => _libraryId == other._libraryId && _valueId == other._valueId;
            public override bool Equals(object obj) => obj is Key other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(_libraryId, _valueId);
            public static bool operator ==(Key left, Key right) => left.Equals(right);
            public static bool operator !=(Key left, Key right) => !left.Equals(right);
        }
        
        private readonly Dictionary<Key, object> _valueMap = new();
        
        public void Dispose() {
            _valueMap.Clear();
        }

        public bool TryGetData<T>(LabelLibraryBase<T> library, int id, out T data) where T : class {
            if (_valueMap.TryGetValue(CreateKey(library, id), out object obj)) {
                data = (T) obj;
                return true;
            }
            
            data = default;
            return false;
        }

        public void SetData<T>(LabelLibraryBase<T> library, int id, T data) where T : class {
            _valueMap[CreateKey(library, id)] = data;
        }

        public bool RemoveData<T>(LabelLibraryBase<T> library, int id) where T : class {
            return _valueMap.Remove(CreateKey(library, id));
        }

        private static Key CreateKey<T>(LabelLibraryBase<T> library, int valueId) where T : class {
            return new Key(library.GetInstanceID(), valueId);
        }
    }
    
}