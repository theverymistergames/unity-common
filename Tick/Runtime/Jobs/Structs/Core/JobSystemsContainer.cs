using System;
using System.Collections.Generic;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public sealed class JobSystemsContainer : IJobSystemProvider {

        private readonly List<IJobSystemBase> _jobSystems;
        private readonly List<Type> _jobSystemsTypes;

        public JobSystemsContainer(List<IJobSystemBase> jobSystems) {
            _jobSystems = jobSystems;
            _jobSystemsTypes = new List<Type>(jobSystems.Count);

            for (int i = 0; i < jobSystems.Count; i++) {
                _jobSystemsTypes[i] = jobSystems[i].GetType();
            }
        }

        public void Initialize(ITimeSource timeSource, IJobIdFactory jobIdFactory) {
            for (int i = 0; i < _jobSystems.Count; i++) {
                _jobSystems[i].Initialize(timeSource, jobIdFactory);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _jobSystems.Count; i++) {
                _jobSystems[i].DeInitialize();
            }

            _jobSystems.Clear();
            _jobSystemsTypes.Clear();
        }

        public S GetJobSystem<S, T>() where S : class, IJobSystem<T> {
            int index = _jobSystemsTypes.IndexOf(typeof(S));
            return index < 0 ? null : _jobSystems[index] as S;
        }

        public S GetJobSystem<S>() where S : class, IJobSystem {
            int index = _jobSystemsTypes.IndexOf(typeof(S));
            return index < 0 ? null : _jobSystems[index] as S;
        }
    }

}
