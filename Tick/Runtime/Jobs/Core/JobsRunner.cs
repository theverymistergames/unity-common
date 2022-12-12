using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs {

    internal sealed class JobsRunner : MonoBehaviour, IJobSystemProviders {

        [SerializeReference] [SubclassSelector]
        private IJobSystemBase[] _jobSystems = {
            //new JobSystemAction(),
            new JobSystemDelay(),
            //new JobSystemSequence(),
        };

        private readonly Dictionary<PlayerLoopStage, JobSystemsContainer> _jobSystemProviders = new Dictionary<PlayerLoopStage, JobSystemsContainer>();
        private readonly IJobIdFactory _jobIdFactory = new NextJobIdFactory();

        private void Awake() {
            var stages = PlayerLoopStages.All;
            for (int i = 0; i < stages.Length; i++) {
                var stage = stages[i];

                var provider = new JobSystemsContainer(TimeSources.Get(stage), CreateNewJobSystems());
                provider.Initialize(_jobIdFactory);

                _jobSystemProviders.Add(stage, provider);
            }

            JobSystems.InjectProvider(this);
        }

        private void OnDestroy() {
            var stages = PlayerLoopStages.All;
            for (int i = 0; i < stages.Length; i++) {
                var stage = stages[i];
                _jobSystemProviders[stage].DeInitialize();
            }

            _jobSystemProviders.Clear();
        }

        public IJobSystemProvider GetProvider(PlayerLoopStage stage) {
            return _jobSystemProviders[stage];
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
