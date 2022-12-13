﻿using System;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;

namespace Utils {

    public class JobSystemProviders : IJobSystemProviders {

        private readonly Func<PlayerLoopStage, IJobSystemProvider> _getStagedProvider;
        private readonly Func<IJobSystemProvider> _getProvider;

        public JobSystemProviders(Func<PlayerLoopStage, IJobSystemProvider> getStagedProvider, Func<IJobSystemProvider> getProvider) {
            _getStagedProvider = getStagedProvider;
            _getProvider = getProvider;
        }

        public IJobSystemProvider GetStagedProvider(PlayerLoopStage stage) {
            return _getStagedProvider.Invoke(stage);
        }

        public IJobSystemProvider GetProvider() {
            return _getProvider.Invoke();
        }
    }

    public class JobSystemProvider : IJobSystemProvider {

        private readonly Func<IJobSystemReadOnly> _getJobSystem;

        public JobSystemProvider(Func<IJobSystemReadOnly> getJobSystem) {
            _getJobSystem = getJobSystem;
        }

        public S GetJobSystem<S>() where S : class, IJobSystemReadOnly {
            return _getJobSystem.Invoke() as S;
        }
    }

}