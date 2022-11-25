namespace MisterGames.Tick.Jobs {
    
    internal sealed class WaitAllJob : IJob {

        public bool IsCompleted {
            get {
                if (_isActive) _isCompleted = _jobObserver.IsCompleted;
                return _isCompleted;
            }
        }

        public float Progress {
            get {
                if (_isActive) _progress = _jobObserver.Progress;
                return _progress;
            }
        }

        private readonly JobObserver _jobObserver = new JobObserver(JobObserver.ProgressMode.TotalObservedProgress);
        private bool _isActive;
        private bool _isCompleted;
        private float _progress;

        public WaitAllJob(params IJobReadOnly[] jobs) {
            _jobObserver.ObserveAll(jobs);
            _isCompleted = _jobObserver.IsCompleted;
            _progress = _jobObserver.Progress;
        }

        public void Start() {
            _isCompleted = _jobObserver.IsCompleted;
            _progress = _jobObserver.Progress;

            _isActive = !_isCompleted;
        }

        public void Stop() {
            _isActive = false;
        }
    }

    internal sealed class WaitJob : IJob {

        public bool IsCompleted {
            get {
                if (_isActive) _isCompleted = _waitForJob.IsCompleted;
                return _isCompleted;
            }
        }

        public float Progress {
            get {
                if (_isActive) _progress = _waitForJob.Progress;
                return _progress;
            }
        }

        private readonly IJobReadOnly _waitForJob;
        private float _progress;
        private bool _isActive;
        private bool _isCompleted;

        public WaitJob(IJobReadOnly job) {
            _waitForJob = job;
            _isCompleted = _waitForJob.IsCompleted;
            _progress = _waitForJob.Progress;
        }

        public void Start() {
            _isCompleted = _waitForJob.IsCompleted;
            _progress = _waitForJob.Progress;

            _isActive = !_isCompleted;
        }

        public void Stop() {
            _isActive = false;
        }
    }

    internal sealed class WaitJobResult<R> : IJob<R> {

        public bool IsCompleted => _waitJob.IsCompleted;
        public float Progress => _waitJob.Progress;
        public R Result => _resultJob.Result;

        private readonly WaitJob _waitJob;
        private readonly IJobReadOnly<R> _resultJob;

        public WaitJobResult(IJobReadOnly<R> job) {
            _waitJob = new WaitJob(job);
            _resultJob = job;
        }

        public void Start() {
            _waitJob.Start();
        }

        public void Stop() {
            _waitJob.Stop();
        }
    }
}
