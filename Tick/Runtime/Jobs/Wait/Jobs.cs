using UnityEngine;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static ReadOnlyJob Wait(ReadOnlyJob job) {
            return JobSystems.Get<JobSystemWait>().CreateJob(job);
        }

        public static ReadOnlyJob Wait(AsyncOperation asyncOperation) {
            return JobSystems.Get<JobSystemAsyncOperation>().CreateJob(asyncOperation);
        }

    }

}
