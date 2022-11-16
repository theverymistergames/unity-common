using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class WaitFramesJob : IJob, IUpdate {

        public bool IsCompleted => _isCompleted;

        private readonly int _waitFrames;

        private int _frameTimer;
        private bool _isUpdating;
        private bool _isCompleted;

        public WaitFramesJob(int waitFrames) {
            _waitFrames = waitFrames;
        }

        public void Start() {
            _isCompleted = _frameTimer >= _waitFrames;
            _isUpdating = !_isCompleted;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _frameTimer++;
            if (_frameTimer < _waitFrames) return;

            _isCompleted = true;
            _isUpdating = false;
        }
    }
    
}
