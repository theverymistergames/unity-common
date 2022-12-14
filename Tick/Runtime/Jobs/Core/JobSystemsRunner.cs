using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs {

    [DefaultExecutionOrder(-9999)]
    internal sealed class JobSystemsRunner : MonoBehaviour, IJobSystemProviders {

        [SerializeField]
        private PlayerLoopStage[] _preInitializeJobSystemsForStages = {
            PlayerLoopStage.Update,
        };

        private readonly IJobSystem[] _jobSystems = {
            new JobSystemAction(),
            new JobSystemDelay(),
            new JobSystemEachFrame(),
            new JobSystemEachFrameWhile(),
            new JobSystemSchedule(),
            new JobSystemAsyncOperation(),
            new JobSystemWait(),
            new JobSystemWaitFrames(),
            new JobSystemSequence(),
        };

        private readonly IJobIdFactory _jobIdFactory = new NextJobIdFactory();
        private readonly Dictionary<PlayerLoopStage, JobSystemContainer> _stagedJobSystemContainers = new Dictionary<PlayerLoopStage, JobSystemContainer>();
        private JobSystemContainer _nonUpdatableJobSystemContainer;

        private void Awake() {
            _nonUpdatableJobSystemContainer = CreateNonUpdatableJobSystemContainer();

            for (int i = 0; i < _preInitializeJobSystemsForStages.Length; i++) {
                var stage = _preInitializeJobSystemsForStages[i];
                _stagedJobSystemContainers.Add(stage, CreateJobSystemContainerForStage(stage));
            }

            JobSystems.InjectProvider(this);
        }

        private void OnDestroy() {
            _nonUpdatableJobSystemContainer.DeInitialize();

            foreach (var (stage, container) in _stagedJobSystemContainers) {
                container.DeInitialize();
                container.UnsubscribeFromTimeSource(TimeSources.Get(stage));
            }

            _stagedJobSystemContainers.Clear();
        }

        public IJobSystemProvider GetStagedProvider(PlayerLoopStage stage) {
            if (_stagedJobSystemContainers.TryGetValue(stage, out var container)) return container;

            var newContainer = CreateJobSystemContainerForStage(stage);
            _stagedJobSystemContainers[stage] = newContainer;

            return newContainer;
        }

        public IJobSystemProvider GetProvider() {
            return _nonUpdatableJobSystemContainer;
        }

        private JobSystemContainer CreateNonUpdatableJobSystemContainer() {
            var container = new JobSystemContainer(CreateNonUpdatableJobSystems());
            container.Initialize(_jobIdFactory);
            return container;
        }

        private JobSystemContainer CreateJobSystemContainerForStage(PlayerLoopStage stage) {
            var container = new JobSystemContainer(CreateUpdatableJobSystems());
            container.Initialize(_jobIdFactory);
            container.SubscribeToTimeSource(TimeSources.Get(stage));
            return container;
        }

        private List<IJobSystem> CreateUpdatableJobSystems() {
            var jobSystems = new List<IJobSystem>(_jobSystems.Length);
            for (int i = 0; i < _jobSystems.Length; i++) {
                var jobSystemSample = _jobSystems[i];
                if (jobSystemSample is not IUpdate) continue;

                jobSystems.Add(CreateInstanceOf(jobSystemSample));
            }
            return jobSystems;
        }

        private List<IJobSystem> CreateNonUpdatableJobSystems() {
            var jobSystems = new List<IJobSystem>(_jobSystems.Length);
            for (int i = 0; i < _jobSystems.Length; i++) {
                var jobSystemSample = _jobSystems[i];
                if (jobSystemSample is IUpdate) continue;

                jobSystems.Add(CreateInstanceOf(jobSystemSample));
            }
            return jobSystems;
        }

        private static T CreateInstanceOf<T>(T sample) where T : class {
            return Activator.CreateInstance(sample.GetType()) as T;
        }
    }

}
