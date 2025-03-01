using UnityEngine;

namespace MisterGames.Common.Inputs {
    
    public interface IGamepadVibration {
        
        void Register(object source, int priority);
        void Unregister(object source);
        
        void SetTwoMotors(object source, Vector2 frequency, float weight = 1f);
        void SetMotor(object source, GamepadMotor motor, float frequency, float weight = 1f);
    }
    
}