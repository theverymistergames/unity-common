using System.Collections.Generic;

namespace MisterGames.Tick.Jobs.Structs {

    public sealed class JobsDataContainer<T> where T : struct {

        public int Count => _keys.Count;
        public IReadOnlyList<int> Keys => _keys;

        public T this[int index] {
            get => _values[index];
            set => _values[index] = value;
        }

        private readonly List<int> _keys = new List<int>();
        private readonly List<T> _values = new List<T>();

        public void Add(int jobId, T data) {
            _keys.Add(jobId);
            _values.Add(data);
        }

        public void RemoveAt(int index) {
            _keys.RemoveAt(index);
            _values.RemoveAt(index);
        }

        public int IndexOf(int jobId) {
            return _keys.IndexOf(jobId);
        }

        public int LastIndexOf(int jobId) {
            return _keys.LastIndexOf(jobId);
        }

        public void Clear() {
            _keys.Clear();
            _values.Clear();
        }
    }
}
