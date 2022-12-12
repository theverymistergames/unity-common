using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs {

    internal sealed class JobSystemsRunner : MonoBehaviour, IJobSystemProviders {

        [SerializeField]
        private PlayerLoopStage[] _preInitializeJobSystemsForStages = {
            PlayerLoopStage.Update,
        };

        [SerializeReference] [SubclassSelector]
        private IJobSystem[] _jobSystems = {
            new JobSystemAction(),
            new JobSystemDelay(),
            new JobSystemSequence(),
        };

        private readonly Dictionary<PlayerLoopStage, JobSystemContainer> _jobSystemContainers = new Dictionary<PlayerLoopStage, JobSystemContainer>();
        private readonly IJobIdFactory _jobIdFactory = new NextJobIdFactory();

        private void Awake() {
            for (int i = 0; i < _preInitializeJobSystemsForStages.Length; i++) {
                var stage = _preInitializeJobSystemsForStages[i];
                _jobSystemContainers.Add(stage, CreateJobSystemContainerForStage(stage));
            }

            JobSystems.InjectProvider(this);
        }

        private void OnDestroy() {
            foreach (var container in _jobSystemContainers.Values) {
                container.DeInitialize();
            }

            _jobSystemContainers.Clear();
        }

        public IJobSystemProvider GetProvider(PlayerLoopStage stage) {
            if (_jobSystemContainers.TryGetValue(stage, out var container)) return container;

            var newContainer = CreateJobSystemContainerForStage(stage);
            _jobSystemContainers[stage] = newContainer;

            return newContainer;
        }

        private JobSystemContainer CreateJobSystemContainerForStage(PlayerLoopStage stage) {
            var container = new JobSystemContainer(TimeSources.Get(stage), CreateNewJobSystems());
            container.Initialize(_jobIdFactory);

            return container;
        }

        private List<IJobSystem> CreateNewJobSystems() {
            var jobSystems = new List<IJobSystem>(_jobSystems.Length);
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
