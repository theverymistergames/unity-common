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

        private readonly List<JobSystemRunner> _jobSystemRunners = new List<JobSystemRunner>();
        private readonly Dictionary<ITimeSource, IJobSystemProvider> _jobSystemProviders = new Dictionary<ITimeSource, IJobSystemProvider>();
        private readonly IJobIdFactory _jobIdFactory = new NextJobIdFactory();

        private void Awake() {
            JobSystemProviders.Instance = this;

            SetupJobSystems();
        }

        private void OnDestroy() {
            for (int i = 0; i < _jobSystemRunners.Count; i++) {
                _jobSystemRunners[i].DeInitialize();
            }
            _jobSystemRunners.Clear();
        }

        private void OnEnable() {
            for (int i = 0; i < _jobSystemRunners.Count; i++) {
                _jobSystemRunners[i].Enable();
            }
        }

        private void OnDisable() {
            for (int i = 0; i < _jobSystemRunners.Count; i++) {
                _jobSystemRunners[i].Disable();
            }
        }

        IJobSystemProvider IJobSystemProviders.GetProvider(ITimeSource timeSource) {
            return _jobSystemProviders[timeSource];
        }

        private void SetupJobSystems() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                var timeSource = _timeDomains[i].Source;
                var runner = new JobSystemRunner(timeSource, CreateNewJobSystems());

                runner.Initialize(_jobIdFactory);

                _jobSystemProviders.Add(timeSource, runner);
                _jobSystemRunners.Add(runner);
            }
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
