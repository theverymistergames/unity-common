using System;

namespace MisterGames.Tick.Jobs {

    public readonly struct Job : IEquatable<Job>, IEquatable<ReadOnlyJob> {

        public bool IsCompleted => !_hasSystem || _system.IsJobCompleted(id);

        internal readonly int id;
        private readonly IJobSystem _system;
        private readonly bool _hasSystem;

        public Job(int id, IJobSystem system) {
            this.id = id;

            _system = system;
            _hasSystem = _system is not null;
        }

        public void Start() {
            if (_hasSystem) _system.StartJob(id);
        }

        public void Stop() {
            if (_hasSystem) _system.StopJob(id);
        }

        public bool Equals(Job other) {
            return id == other.id;
        }

        public bool Equals(ReadOnlyJob other) {
            return id == other.id;
        }

        public override bool Equals(object obj) {
            return obj switch {
                Job job => Equals(job),
                ReadOnlyJob readOnlyJob => Equals(readOnlyJob),
                _ => false
            };
        }

        public override int GetHashCode() {
            return id;
        }

        public static bool operator ==(Job left, Job right) {
            return left.Equals(right);
        }

        public static bool operator !=(Job left, Job right) {
            return !left.Equals(right);
        }

        public static bool operator ==(Job left, ReadOnlyJob right) {
            return left.Equals(right);
        }

        public static bool operator !=(Job left, ReadOnlyJob right) {
            return !left.Equals(right);
        }

        public static implicit operator ReadOnlyJob(Job job) {
            return new ReadOnlyJob(job.id, job._system);
        }
    }

    public readonly struct ReadOnlyJob : IEquatable<Job>, IEquatable<ReadOnlyJob> {

        public bool IsCompleted => !_hasSystem || _system.IsJobCompleted(id);

        internal readonly int id;
        private readonly IJobSystem _system;
        private readonly bool _hasSystem;

        public ReadOnlyJob(int id, IJobSystem system) {
            this.id = id;

            _system = system;
            _hasSystem = _system is not null;
        }

        public bool Equals(Job other) {
            return id == other.id;
        }

        public bool Equals(ReadOnlyJob other) {
            return id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is Job job && Equals(job) ||
                   obj is ReadOnlyJob readOnlyJob && Equals(readOnlyJob);
        }

        public override int GetHashCode() {
            return id;
        }

        public static bool operator ==(ReadOnlyJob left, ReadOnlyJob right) {
            return left.Equals(right);
        }

        public static bool operator !=(ReadOnlyJob left, ReadOnlyJob right) {
            return !left.Equals(right);
        }

        public static bool operator ==(ReadOnlyJob left, Job right) {
            return left.Equals(right);
        }

        public static bool operator !=(ReadOnlyJob left, Job right) {
            return !left.Equals(right);
        }
    }

}
