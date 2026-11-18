using UnityEngine;

namespace MisterGames.Common.Inputs {
    
    public interface IGamepadVibration {
        
        void Register(object source, int priority);
        void Unregister(object source);
        
        void SetTwoMotors(object source, Vector2 frequency, float weightLeft = 1f, float weightRight = 1f);
        void SetMotor(object source, GamepadSide side, float frequency, float weight = 1f);
    }
    
}