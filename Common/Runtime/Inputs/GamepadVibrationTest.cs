using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.Common.Inputs {
    
    public class GamepadVibrationTest : MonoBehaviour {
        
        public OscillatedCurve[] clips;
        
        private void OnEnable() {
            DeviceService.Instance.GamepadVibration.Register(this, 0);
        }

        private void OnDisable() {
            DeviceService.Instance.GamepadVibration.Unregister(this);
        }

        public void Update() {
            //DeviceService.Instance.GamepadVibration.SetTwoMotors(this, new Vector2(low, high));
        }
    }
}