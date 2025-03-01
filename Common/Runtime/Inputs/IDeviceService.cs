using System;
using MisterGames.Common.Inputs.DualSense;
using UnityEngine.InputSystem;

namespace MisterGames.Common.Inputs {
    
    public interface IDeviceService {
        
        event Action<DeviceType> OnDeviceChanged;
        
        DeviceType CurrentDevice { get; }
        IGamepadVibration GamepadVibration { get; }
        IDualSenseAdapter DualSenseAdapter { get; }
        bool TryGetGamepad(out Gamepad gamepad);
    }
    
}