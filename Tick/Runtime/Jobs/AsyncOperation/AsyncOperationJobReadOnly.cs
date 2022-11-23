using UnityEngine;

namespace MisterGames.Tick.Jobs {

    internal sealed class AsyncOperationJobReadOnly : IJobReadOnly {

        public bool IsCompleted => _asyncOperation.isDone;

        private readonly AsyncOperation _asyncOperation;

        public AsyncOperationJobReadOnly(AsyncOperation asyncOperation) {
            _asyncOperation = asyncOperation;
        }
    }
}
