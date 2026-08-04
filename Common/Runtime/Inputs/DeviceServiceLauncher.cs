using MisterGames.Common.Inputs.DualSense;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Inputs {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class DeviceServiceLauncher : MonoBehaviour {
        
        [SerializeField] private GamepadVibration _gamepadVibration;
        [SerializeField] private DualSenseAdapter _dualSenseAdapter;
        
        private readonly DeviceService _deviceService = new();
        
        private void Awake() {
            _deviceService.Initialize(_gamepadVibration, _dualSenseAdapter);
            Services.Register<IDeviceService>(_deviceService);
        }

        private void OnDestroy() {
            Services.Unregister(_deviceService);
            _deviceService.Dispose();
        }
    }
    
}