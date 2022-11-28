namespace MisterGames.Tick.Jobs.Structs {

    public readonly struct Job {

        public bool IsCompleted => _system?.IsJobCompleted(_id) ?? true;

        private readonly IJobSystemBase _system;
        private readonly int _id;

        public Job(int id, IJobSystemBase system) {
            _system = system;
            _id = id;
        }

        public void Start() {
            _system?.StartJob(_id);
        }

        public void Stop() {
            _system?.StopJob(_id);
        }

        public bool Equals(Job other) {
            return _id == other._id;
        }

        public override bool Equals(object obj) {
            return obj is Job other && Equals(other);
        }

        public override int GetHashCode() {
            return _id;
        }
    }

}
