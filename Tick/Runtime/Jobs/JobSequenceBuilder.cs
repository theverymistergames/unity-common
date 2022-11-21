using System.Collections.Generic;

namespace MisterGames.Tick.Jobs {

    public class JobSequenceBuilder {

        private readonly List<IJob> _jobs = new List<IJob>();

        internal JobSequenceBuilder() { }

        public JobSequenceBuilder Add(IJob job) {
            _jobs.Add(job);
            return this;
        }

        public IJob Create() {
            return new JobSequence(_jobs);
        }
    }

}
