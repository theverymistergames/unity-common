using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using MisterGames.Fsm.Core;
using MisterGames.Tick.Utils;
using UnityEngine;

namespace MisterGames.Fsm.Basics {

    public sealed class DelayedTransition : FsmTransition {
        
        [SerializeField] private float _delay;

        private IJob _delayJob;
        private ITimeSource _timeSource;

        protected override void OnAttach(StateMachineRunner runner) {
            _timeSource = runner.TimeSource;
        }

        protected override void OnDetach() {
            _delayJob?.Stop();
        }

        protected override void OnEnterSourceState() {
            _delayJob?.Stop();

            _delayJob = JobSequence.Create()
                .Delay(_delay)
                .Action(Transit)
                .StartFrom(_timeSource);
        }

        protected override void OnExitSourceState() {
            _delayJob?.Stop();
        }
    }

}
