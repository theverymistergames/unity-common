using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class WaitFramesJob : IJob, IUpdate {

        public bool IsCompleted => _frameTimer >= _waitFrames;

        private readonly int _waitFrames;

        private int _frameTimer;
        private bool _isUpdating;

        public WaitFramesJob(int waitFrames) {
            _waitFrames = waitFrames;
        }

        public void Start() {
            _isUpdating = _frameTimer < _waitFrames;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _frameTimer++;
            if (_frameTimer < _waitFrames) return;

            _isUpdating = false;
        }
    }
    
}
