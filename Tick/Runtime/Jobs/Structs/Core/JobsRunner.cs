using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs.Structs {

    internal sealed class JobsRunner : MonoBehaviour, IJobSystemProviders {

        [SerializeField]
        private TimeDomain[] _timeDomains;

        [SerializeReference] [SubclassSelector]
        private IJobSystemBase[] _jobSystems = {
            new JobSystemDelay(),
            new JobSystemSequence(),
        };

        private readonly Dictionary<ITimeSource, JobSystemsContainer> _jobSystemProviders = new Dictionary<ITimeSource, JobSystemsContainer>();
        private readonly IJobIdFactory _jobIdFactory = new NextJobIdFactory();

        private void Awake() {
            Jobs.InjectJobSystemProviders(this);

            for (int i = 0; i < _timeDomains.Length; i++) {
                var timeSource = _timeDomains[i].Source;

                var provider = new JobSystemsContainer(timeSource, CreateNewJobSystems());
                provider.Initialize(_jobIdFactory);

                _jobSystemProviders.Add(timeSource, provider);
            }
        }

        private void OnDestroy() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                var timeSource = _timeDomains[i].Source;
                _jobSystemProviders[timeSource].DeInitialize();
            }

            _jobSystemProviders.Clear();
        }

        public IJobSystemProvider GetProvider(ITimeSource timeSource) {
            return _jobSystemProviders[timeSource];
        }

        private List<IJobSystemBase> CreateNewJobSystems() {
            var jobSystems = new List<IJobSystemBase>(_jobSystems.Length);
            for (int i = 0; i < _jobSystems.Length; i++) {
                jobSystems.Add(CreateInstanceOf(_jobSystems[i]));
            }
            return jobSystems;
        }

        private static T CreateInstanceOf<T>(T sample) where T : class {
            return Activator.CreateInstance(sample.GetType()) as T;
        }
    }

}
