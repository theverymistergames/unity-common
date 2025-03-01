using UnityEngine;

namespace MisterGames.Common.Inputs {
    
    public interface IGamepadVibration {
        
        void Register(object source, int priority);
        void Unregister(object source);
        
        void SetTwoMotors(object source, Vector2 frequency, float weight = 1f);
        void SetLeftMotor(object source, float frequency, float weight = 1f);
        void SetRightMotor(object source, float frequency, float weight = 1f);
    }
    
}