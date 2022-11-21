using UnityEngine;

namespace MisterGames.Tick.Jobs {

    public static class AsyncOperationExtensions {

        public static IJobReadOnly AsReadOnlyJob(this AsyncOperation asyncOperation) {
            return new AsyncOperationJobReadOnly(asyncOperation);
        }

    }

}
