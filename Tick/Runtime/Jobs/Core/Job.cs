using System;

namespace MisterGames.Tick.Jobs {

    public readonly struct Job : IEquatable<Job> {

        public bool IsCompleted => !_hasSystem || _system.IsJobCompleted(_id);

        private readonly int _id;
        private readonly IJobSystem _system;
        private readonly bool _hasSystem;

        public Job(int id, IJobSystem system) {
            _id = id;
            _system = system;
            _hasSystem = _system is not null;
        }

        public Job Start() {
            if (_hasSystem) _system.StartJob(_id);
            return this;
        }

        public Job Stop() {
            if (_hasSystem) _system.StopJob(_id);
            return this;
        }

        public void Dispose() {
            if (_hasSystem) _system.DisposeJob(_id);
        }

        public bool Equals(Job other) {
            return _id == other._id;
        }

        public override bool Equals(object obj) {
            return obj is Job job && Equals(job);
        }

        public override int GetHashCode() {
            return _id;
        }

        public static bool operator ==(Job left, Job right) {
            return left.Equals(right);
        }

        public static bool operator !=(Job left, Job right) {
            return !left.Equals(right);
        }

        public static implicit operator ReadOnlyJob(Job job) {
            return new ReadOnlyJob(job._id, job._system);
        }
    }

    public readonly struct ReadOnlyJob : IEquatable<ReadOnlyJob> {

        public bool IsCompleted => !_hasSystem || _system.IsJobCompleted(_id);

        private readonly int _id;
        private readonly IJobSystem _system;
        private readonly bool _hasSystem;

        public ReadOnlyJob(int id, IJobSystem system) {
            _id = id;
            _system = system;
            _hasSystem = _system is not null;
        }

        public void Dispose() {
            if (_hasSystem) _system.DisposeJob(_id);
        }

        public bool Equals(ReadOnlyJob other) {
            return _id == other._id;
        }

        public override bool Equals(object obj) {
            return obj is ReadOnlyJob job && Equals(job);
        }

        public override int GetHashCode() {
            return _id;
        }

        public static bool operator ==(ReadOnlyJob left, ReadOnlyJob right) {
            return left.Equals(right);
        }

        public static bool operator !=(ReadOnlyJob left, ReadOnlyJob right) {
            return !left.Equals(right);
        }
    }

}
