using System;
using System.Collections.Generic;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public sealed class JobSystemsContainer : IJobSystemProvider {

        private readonly ITimeSource _timeSource;
        private readonly List<IJobSystemBase> _jobSystems;
        private readonly Dictionary<Type, IJobSystemBase> _jobSystemsTypes;

        public JobSystemsContainer(ITimeSource timeSource, List<IJobSystemBase> jobSystems) {
            _timeSource = timeSource;
            _jobSystems = jobSystems;
            _jobSystemsTypes = new Dictionary<Type, IJobSystemBase>(jobSystems.Count);
        }

        public void Initialize(IJobIdFactory jobIdFactory) {
            for (int i = 0; i < _jobSystems.Count; i++) {
                var jobSystem = _jobSystems[i];
                jobSystem.Initialize(jobIdFactory);

                _jobSystemsTypes[jobSystem.GetType()] = jobSystem;
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
            _jobSystemsTypes.Clear();
        }

        public S GetJobSystem<S, T>() where S : class, IJobSystem<T> {
            return _jobSystemsTypes[typeof(S)] as S;
        }

        public S GetJobSystem<S>() where S : class, IJobSystem {
            return _jobSystemsTypes[typeof(S)] as S;
        }
    }

}
