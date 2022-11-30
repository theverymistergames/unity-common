using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs.Structs {

    internal sealed class JobsRunner : MonoBehaviour, IJobSystemProviders {

        [SerializeField]
        private TimeDomain[] _timeDomains;

        [SerializeReference] [SubclassSelector] [BeginReadOnlyGroup]
        private IJobSystemBase[] _defaultJobSystems = {
            new JobSystemDelay(),
        };

        [SerializeReference] [SubclassSelector] [EndReadOnlyGroup]
        private IJobSystemBase[] _customJobSystems;

        private readonly Dictionary<ITimeSource, JobSystemsContainer> _jobSystemProviders = new Dictionary<ITimeSource, JobSystemsContainer>();
        private readonly IJobIdFactory _jobIdFactory = new NextJobIdFactory();

        private void Awake() {
            Jobs.JobSystemProviders = this;

            for (int i = 0; i < _timeDomains.Length; i++) {
                var timeSource = _timeDomains[i].Source;

                var provider = new JobSystemsContainer(CreateNewJobSystems());
                provider.Initialize(timeSource, _jobIdFactory);

                _jobSystemProviders.Add(timeSource, provider);
            }
        }

        private void OnDestroy() {
            foreach (var provider in _jobSystemProviders.Values) {
                provider.DeInitialize();
            }
            _jobSystemProviders.Clear();
        }

        public IJobSystemProvider GetProvider(ITimeSource timeSource) {
            return _jobSystemProviders[timeSource];
        }

        private List<IJobSystemBase> CreateNewJobSystems() {
            var jobSystems = new List<IJobSystemBase>(_defaultJobSystems.Length + _customJobSystems.Length);

            for (int i = 0; i < _defaultJobSystems.Length; i++) {
                jobSystems.Add(CreateInstanceOf(_defaultJobSystems[i]));
            }

            for (int i = 0; i < _customJobSystems.Length; i++) {
                jobSystems.Add(CreateInstanceOf(_customJobSystems[i]));
            }

            return jobSystems;
        }

        private static T CreateInstanceOf<T>(T sample) where T : class {
            return Activator.CreateInstance(sample.GetType()) as T;
        }
    }

}
