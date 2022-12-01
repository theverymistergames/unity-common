using System.Collections.Generic;

namespace MisterGames.Tick.Jobs.Structs {

    public sealed class JobsDataContainer<T> where T : struct {

        public int Count => _jobsData.Count;

        public T this[int index] {
            get => _jobsData[index];
            set => _jobsData[index] = value;
        }

        private readonly List<int> _jobIds = new List<int>();
        private readonly List<T> _jobsData = new List<T>();

        public void Add(int jobId, T data) {
            _jobIds.Add(jobId);
            _jobsData.Add(data);
        }

        public void RemoveAt(int index) {
            _jobIds.RemoveAt(index);
            _jobsData.RemoveAt(index);
        }

        public int IndexOf(int jobId) {
            return _jobIds.IndexOf(jobId);
        }

        public int LastIndexOf(int jobId) {
            return _jobIds.LastIndexOf(jobId);
        }

        public void Clear() {
            _jobIds.Clear();
            _jobsData.Clear();
        }
    }
}
