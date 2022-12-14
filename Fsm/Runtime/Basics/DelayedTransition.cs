using MisterGames.Fsm.Core;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using UnityEngine;

namespace MisterGames.Fsm.Basics {

    public sealed class DelayedTransition : FsmTransition {
        
        [SerializeField] private float _delay;

        private Job _delayJob;
        private PlayerLoopStage _timeSourceStage;

        protected override void OnAttach(StateMachineRunner runner) {
            _timeSourceStage = runner.TimeSourceStage;
        }

        protected override void OnDetach() {
            _delayJob.Dispose();
        }

        protected override void OnEnterSourceState() {
            _delayJob.Dispose();

            _delayJob = JobSequence.Create(_timeSourceStage)
                .Delay(_delay)
                .Action(Transit)
                .Push()
                .Start();
        }

        protected override void OnExitSourceState() { }
    }

}
