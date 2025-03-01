using UnityEngine;

namespace MisterGames.Common.Inputs {
    
    public class GamepadVibrationTest : MonoBehaviour {
        
        [Range(0f, 1f)] public float low;
        [Range(0f, 1f)] public float high;
        
        private void OnEnable() {
            DeviceService.Instance.GamepadVibration.Register(this, 0);
        }

        private void OnDisable() {
            DeviceService.Instance.GamepadVibration.Unregister(this);
        }

        public void Update() {
            DeviceService.Instance.GamepadVibration.SetFrequency(this, new Vector2(low, high));
        }
    }
}