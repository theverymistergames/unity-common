using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public interface IJumpOverride {
        
        bool OnJumpRequested(ref float impulseDelay);
        
        bool OnJumpImpulseRequested(ref Vector3 impulse);
    }
    
}