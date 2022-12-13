using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public sealed class JobSystemContainer : IJobSystemProvider {

        private readonly DictionaryList<Type, IJobSystemReadOnly> _jobSystems;

        public JobSystemContainer(IReadOnlyList<IJobSystemReadOnly> jobSystems) {
            _jobSystems = new DictionaryList<Type, IJobSystemReadOnly>(jobSystems.Count);

            for (int i = 0; i < jobSystems.Count; i++) {
                var jobSystem = jobSystems[i];
                _jobSystems.Add(jobSystem.GetType(), jobSystem);
            }
        }

        public void Initialize(IJobIdFactory jobIdFactory) {
            for (int i = 0; i < _jobSystems.Count; i++) {
                var jobSystem = _jobSystems.Values[i];
                jobSystem.Initialize(jobIdFactory);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _jobSystems.Count; i++) {
                var jobSystem = _jobSystems.Values[i];
                jobSystem.DeInitialize();
            }

            _jobSystems.Clear();
        }

        public void SubscribeToTimeSource(ITimeSource timeSource) {
            for (int i = 0; i < _jobSystems.Count; i++) {
                if (_jobSystems.Values[i] is IUpdate update) timeSource.Subscribe(update);
            }
        }

        public void UnsubscribeFromTimeSource(ITimeSource timeSource) {
            for (int i = 0; i < _jobSystems.Count; i++) {
                if (_jobSystems.Values[i] is IUpdate update) timeSource.Unsubscribe(update);
            }
        }

        public S GetJobSystem<S>() where S : class, IJobSystemReadOnly {
            int index = _jobSystems.Keys.IndexOf(typeof(S));
            if (index < 0) return null;

            return _jobSystems.Values[index] as S;
        }
    }

}
