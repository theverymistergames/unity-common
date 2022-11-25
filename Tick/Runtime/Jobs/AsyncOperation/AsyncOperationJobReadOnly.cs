using UnityEngine;

namespace MisterGames.Tick.Jobs {

    internal sealed class AsyncOperationJobReadOnly : IJobReadOnly {

        public bool IsCompleted => _asyncOperation.isDone;
        public float Progress => _asyncOperation.isDone ? 1f : _asyncOperation.progress;

        private readonly AsyncOperation _asyncOperation;

        public AsyncOperationJobReadOnly(AsyncOperation asyncOperation) {
            _asyncOperation = asyncOperation;
        }
    }
}
