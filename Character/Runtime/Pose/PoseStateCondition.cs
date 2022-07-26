namespace MisterGames.Character.Pose {

    public struct PoseStateCondition {

        public bool isGrounded;
        public bool hasCeiling;
        public bool isCrouchInputActive;
        public bool isCrouching;

        public bool Equals(PoseStateCondition other) {
            return isGrounded == other.isGrounded
                   && hasCeiling == other.hasCeiling
                   && isCrouchInputActive == other.isCrouchInputActive
                   && isCrouching == other.isCrouching;
        }

        public override string ToString() {
            return "PoseStateCondition(" +
                   $"grounded {isGrounded}, " +
                   $"ceiling {hasCeiling}, " +
                   $"crouch input {isCrouchInputActive}, " +
                   $"crouch {isCrouching}" +
                   ")";
        }
        
    }

}