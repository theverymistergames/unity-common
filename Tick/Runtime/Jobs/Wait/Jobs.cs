using UnityEngine;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static Job Wait(ReadOnlyJob job) {
            return JobSystems.Get<JobSystemWait>().CreateJob(job);
        }

        public static Job Wait(AsyncOperation asyncOperation) {
            return JobSystems.Get<JobSystemAsyncOperation>().CreateJob(asyncOperation);
        }

        public static JobSequence Wait(this JobSequence jobSequence, ReadOnlyJob waitFor) {
            var job = Wait(waitFor);
            return jobSequence.Add(job);
        }

        public static JobSequence Wait(this JobSequence jobSequence, AsyncOperation asyncOperation) {
            var job = Wait(asyncOperation);
            return jobSequence.Add(job);
        }
    }

}
