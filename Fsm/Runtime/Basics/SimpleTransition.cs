using MisterGames.Fsm.Core;
using UnityEngine;

namespace MisterGames.Fsm.Basics {

    public abstract class SimpleTransition : FsmTransition {
        
        protected override void OnAttach(StateMachineRunner runner) { }
        protected override void OnDetach() { }
        protected override void OnEnterSourceState() { }
        protected override void OnExitSourceState() { }
        
    }

}