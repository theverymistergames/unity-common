using UnityEngine;

namespace MisterGames.Common.Inputs.DualSense {
    
    public interface IDualSenseAdapter {
        
        bool HasController(int index = 0);
        
        ControllerInputState GetInputState(int index = 0);
        
        void SetRumble(Vector2 rumble, int index = 0);
        
    }
    
}