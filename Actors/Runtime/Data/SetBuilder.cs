using System;
using System.Collections.Generic;

namespace MisterGames.Actors
{
    
    public sealed class SetBuilder<T> : ISetBuilder<T> where T : class
    {
        private readonly Dictionary<Type, int> _indexMap = new();
        private readonly List<T> _buffer = new();

        public void Clear() {
            _indexMap.Clear();
            _buffer.Clear();
        }

        public IReadOnlyList<T> GetResultArray() {
            return _buffer;
        }

        public S Get<S>() where S : T {
            return Get(typeof(S)) is S s ? s : default;
        }
        
        public T Get(Type type) {
            return _indexMap.TryGetValue(type, out int index) ? _buffer[index] : null;
        }

        public ISetBuilder<T> Set(IReadOnlyList<T> list) {
            for (int i = 0; i < list?.Count; i++) {
                Set(list[i]);
            }

            return this;
        }

        public ISetBuilder<T> Set(T data) {
            if (data == null) {
                _buffer.Add(null);
                return this;
            }

            var type = data.GetType();
            if (_indexMap.TryGetValue(type, out int index)) {
                _buffer[index] = data;
            }
            else {
                _indexMap[type] = _buffer.Count;
                _buffer.Add(data);
            }
            
            return this;
        }
        
        public ISetBuilder<T> Set<S>() where S : T {
            return Set(typeof(S));
        }

        public ISetBuilder<T> Set(Type type) {
            if (!_indexMap.ContainsKey(type) && 
                typeof(T).IsAssignableFrom(type)) 
            {
                return Set(Activator.CreateInstance(type) as T);
            }
            
            return this;
        }

        public ISetBuilder<T> Remove<S>() where S : T {
            return Remove(typeof(S));
        }

        public ISetBuilder<T> Remove(Type type) {
            if (_indexMap.Remove(type, out int index)) {
                _buffer.RemoveAt(index);
            } 
            
            _indexMap.Clear();
            for (int i = 0; i < _buffer.Count; i++) {
                if (_buffer[i] is var d) _indexMap[d.GetType()] = i;
            }

            return this;
        }
    }
    
}