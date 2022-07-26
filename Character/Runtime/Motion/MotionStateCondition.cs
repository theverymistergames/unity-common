namespace MisterGames.Character.Motion {

    public struct MotionStateCondition {

        public bool isMotionActive;
        public bool isRunActive;
        public bool isCrouchActive;
        public bool isGrounded;

        public bool Equals(MotionStateCondition other) {
            return isMotionActive == other.isMotionActive
                   && isRunActive == other.isRunActive
                   && isCrouchActive == other.isCrouchActive
                   && isGrounded == other.isGrounded;
        }

        public override string ToString() {
            return "MotionStateCondition(" +
                   $"motion {isMotionActive}, " +
                   $"run {isRunActive}, " +
                   $"crouch {isCrouchActive}, " +
                   $"grounded {isGrounded}" +
                   ")";
        }
        
    }

}