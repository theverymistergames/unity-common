using MisterGames.Tick.Jobs;
using UnityEngine;

namespace MisterGames.Tick.Utils {

    public static class AsyncOperationExtensions {

        public static IJobReadOnly AsReadOnlyJob(this AsyncOperation asyncOperation) {
            return new AsyncOperationJobReadOnly(asyncOperation);
        }

    }

}
