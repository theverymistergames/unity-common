using MisterGames.Common.Data;
using MisterGames.Fsm.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public class MotionStateTransition : FsmTransition {

        [SerializeField] private Optional<bool> _isMotionActive;
        [SerializeField] private Optional<bool> _isRunActive;
        [SerializeField] private Optional<bool> _isCrouchActive;
        [SerializeField] private Optional<bool> _isGrounded;

        protected override void OnAttach(StateMachineRunner runner) { }
        protected override void OnDetach() { }
        protected override void OnEnterSourceState() { }
        protected override void OnExitSourceState() { }
        
        public bool CheckCondition(MotionStateCondition condition) {
            var isValid = IsValid && Satisfies(condition);
            if (isValid) Transit();
            return isValid;
        }

        private bool Satisfies(MotionStateCondition condition) {
            return Satisfies(_isMotionActive, condition.isMotionActive)
                   && Satisfies(_isRunActive, condition.isRunActive)
                   && Satisfies(_isCrouchActive, condition.isCrouchActive)
                   && Satisfies(_isGrounded, condition.isGrounded);
        }
        
        private static bool Satisfies(Optional<bool> optional, bool value) {
            return !optional.HasValue || optional.Value.Equals(value);
        }
        
    }

}