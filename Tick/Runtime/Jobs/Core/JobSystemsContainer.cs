using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public sealed class JobSystemsContainer : IJobSystemProvider {

        private readonly ITimeSource _timeSource;
        private readonly DictionaryList<Type, IJobSystemBase> _jobSystems;

        public JobSystemsContainer(ITimeSource timeSource, IReadOnlyList<IJobSystemBase> jobSystems) {
            _timeSource = timeSource;
            _jobSystems = new DictionaryList<Type, IJobSystemBase>(jobSystems.Count);

            for (int i = 0; i < jobSystems.Count; i++) {
                var jobSystem = jobSystems[i];
                _jobSystems.Add(jobSystem.GetType(), jobSystem);
            }
        }

        public void Initialize(IJobIdFactory jobIdFactory) {
            for (int i = 0; i < _jobSystems.Count; i++) {
                var jobSystem = _jobSystems[i];
                jobSystem.Initialize(jobIdFactory);

                if (jobSystem is IUpdate update) _timeSource.Subscribe(update);
            }
        }

        public void DeInitialize() {
            for (int i = _jobSystems.Count - 1; i >= 0; i--) {
                var jobSystem = _jobSystems[i];
                jobSystem.DeInitialize();

                if (jobSystem is IUpdate update) _timeSource.Unsubscribe(update);
            }

            _jobSystems.Clear();
        }

        public S GetJobSystem<S>() where S : class, IJobSystemBase {
            int index = _jobSystems.IndexOf(typeof(S));
            if (index < 0) return null;

            return _jobSystems[index] as S;
        }
    }
}
