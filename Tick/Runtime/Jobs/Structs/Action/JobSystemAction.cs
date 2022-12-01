using System;
using MisterGames.Common.Data;

namespace MisterGames.Tick.Jobs.Structs {

    [Serializable]
    internal sealed class JobSystemAction : IJobSystem<Action> {

        private readonly DictionaryList<int, Action> _actions = new DictionaryList<int, Action>();
        private IJobIdFactory _jobIdFactory;

        public void Initialize(IJobIdFactory jobIdFactory) {
            _jobIdFactory = jobIdFactory;
        }

        public void DeInitialize() {
            _actions.Clear();
        }

        public int CreateJob(Action data) {
            int jobId = _jobIdFactory.CreateNewJobId();
            _actions.Add(jobId, data);
            return jobId;
        }

        public bool IsJobCompleted(int jobId) {
            return _actions.IndexOf(jobId) < 0;
        }

        public void StartJob(int jobId) {
            int index = _actions.IndexOf(jobId);
            if (index < 0) return;

            _actions[index].Invoke();
            _actions.RemoveAt(index);
        }

        public void StopJob(int jobId) { }
    }

}
