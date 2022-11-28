using System;
using System.Collections.Generic;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public sealed class JobSystemRunner : IJobSystemProvider {

        private readonly IJobIdFactory _jobIdFactory;
        private readonly ITimeSource _timeSource;
        private readonly List<IJobSystemBase> _jobSystems;
        private readonly List<Type> _jobSystemsTypes;

        public JobSystemRunner(ITimeSource timeSource, List<IJobSystemBase> jobSystems) {
            _timeSource = timeSource;
            _jobSystems = jobSystems;

            _jobSystemsTypes = new List<Type>(jobSystems.Count);
            for (int i = 0; i < jobSystems.Count; i++) {
                _jobSystemsTypes[i] = _jobSystems[i].GetType();
            }
        }

        public void Initialize(IJobIdFactory jobIdFactory) {
            for (int i = 0; i < _jobSystems.Count; i++) {
                _jobSystems[i].Initialize(jobIdFactory);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _jobSystems.Count; i++) {
                var jobSystem = _jobSystems[i];
                jobSystem.DeInitialize();

                if (jobSystem is IUpdate update) _timeSource.Unsubscribe(update);
            }

            _jobSystems.Clear();
            _jobSystemsTypes.Clear();
        }

        public void Enable() {
            for (int i = 0; i < _jobSystems.Count; i++) {
                var jobSystem = _jobSystems[i];
                if (jobSystem is IUpdate update) _timeSource.Subscribe(update);
            }
        }

        public void Disable() {
            for (int i = 0; i < _jobSystems.Count; i++) {
                var jobSystem = _jobSystems[i];
                if (jobSystem is IUpdate update) _timeSource.Unsubscribe(update);
            }
        }

        public S GetJobSystem<S>() where S : IJobSystem {
            int index = _jobSystemsTypes.IndexOf(typeof(S));
            if (index < 0 || _jobSystems[index] is not S jobSystem) {
                throw new NotSupportedException($"Job system of type {typeof(S)} is not found");
            }

            return jobSystem;
        }

        public S GetJobSystem<S, T>() where S : IJobSystem<T> {
            int index = _jobSystemsTypes.IndexOf(typeof(S));
            if (index < 0 || _jobSystems[index] is not S jobSystem) {
                throw new NotSupportedException($"Job system of type {typeof(S)} is not found");
            }

            return jobSystem;
        }
    }
}
