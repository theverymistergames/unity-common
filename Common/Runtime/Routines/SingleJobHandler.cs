namespace MisterGames.Common.Routines {

    public sealed class SingleJobHandler {

        private IJob _currentJob;

        public void Start(IJob job) {
            Stop();
            _currentJob = job;
            _currentJob.Start();
        }

        public void Stop() {
            _currentJob?.Stop();
        }

        public void Pause() {
            _currentJob?.Pause();
        }

        public void Resume() {
            _currentJob?.Resume();
        }
    }

}