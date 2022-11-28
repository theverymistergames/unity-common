using System.Collections.Generic;

namespace MisterGames.Tick.Jobs.Structs {

    public sealed class JobsContainer<T> where T : struct {

        public int Count => _jobs.Count;

        public T this[int index] {
            get => _jobs[index];
            set => _jobs[index] = value;
        }

        private readonly IJobIdFactory _jobFactory;
        private readonly List<T> _jobs = new List<T>();
        private readonly List<int> _jobIds = new List<int>();

        public JobsContainer(IJobIdFactory jobFactory) {
            _jobFactory = jobFactory;
        }

        public int AddNewJobData(T data) {
            int jobId = _jobFactory.CreateNewJobId();

            _jobIds.Add(jobId);
            _jobs.Add(data);

            return jobId;
        }

        public void RemoveAt(int index) {
            _jobIds.RemoveAt(index);
            _jobs.RemoveAt(index);
        }

        public int IndexOf(int jobId) {
            return _jobIds.IndexOf(jobId);
        }

        public void Clear() {
            _jobIds.Clear();
            _jobs.Clear();
        }
    }
}
