using MisterGames.Common.Data;
using MisterGames.Fsm.Core;
using UnityEngine;

namespace MisterGames.Character.Pose {

    public class PoseStateTransition : FsmTransition {

        [SerializeField] private Optional<bool> _isGrounded;
        [SerializeField] private Optional<bool> _hasCeiling;
        [SerializeField] private Optional<bool> _isCrouchInputActive;
        [SerializeField] private Optional<bool> _isCrouching;

        protected override void OnAttach(StateMachineRunner runner) { }
        protected override void OnDetach() { }
        protected override void OnEnterSourceState() { }
        protected override void OnExitSourceState() { }
        
        public bool CheckCondition(PoseStateCondition condition) {
            var isValid = IsValid && Satisfies(condition);
            if (isValid) Transit();
            return isValid;
        }

        private bool Satisfies(PoseStateCondition condition) {
            return Satisfies(_isGrounded, condition.isGrounded)
                   && Satisfies(_hasCeiling, condition.hasCeiling)
                   && Satisfies(_isCrouchInputActive, condition.isCrouchInputActive)
                   && Satisfies(_isCrouching, condition.isCrouching);
        }
        
        private static bool Satisfies(Optional<bool> optional, bool value) {
            return !optional.HasValue || optional.Value.Equals(value);
        }
        
    }

}