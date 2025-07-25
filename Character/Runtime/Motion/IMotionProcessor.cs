using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public interface IMotionProcessor {

        bool ProcessOrientation(ref Quaternion orientation, out int priority);
        
        void ProcessInputSpeed(ref float speed, float dt);
        void ProcessInputForce(ref Vector3 inputForce, Vector3 desiredVelocity, float dt);
    }
    
}