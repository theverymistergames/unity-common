using MisterGames.Common.Routines;
using MisterGames.Fsm.Core;
using UnityEngine;

namespace MisterGames.Fsm.Basics {

    public sealed class DelayedTransition : FsmTransition {
        
        [SerializeField] private float _delay;

        private readonly SingleJobHandler _handler = new SingleJobHandler();
        private TimeDomain _timeDomain;

        protected override void OnAttach(StateMachineRunner runner) {
            _timeDomain = runner.TimeDomain;
        }

        protected override void OnDetach() {
            _handler.Stop();
        }

        protected override void OnEnterSourceState() {
            Jobs.Do(_timeDomain.Delay(_delay))
                .Then(Transit)
                .StartFrom(_handler);
        }

        protected override void OnExitSourceState() {
            _handler.Stop();
        }

    }

}