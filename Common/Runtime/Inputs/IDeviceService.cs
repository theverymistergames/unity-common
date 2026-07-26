using System;
using MisterGames.Common.Inputs.DualSense;
using UnityEngine.InputSystem;

namespace MisterGames.Common.Inputs {
    
    public interface IDeviceService {
        
        event Action<InputDeviceType> OnDeviceChanged;

        int LastPointerDeviceId { get; }
        InputDeviceType CurrentDevice { get; }
        GamepadType GamepadType { get; }
        IGamepadVibration GamepadVibration { get; }
        IDualSenseAdapter DualSenseAdapter { get; }
        bool AnyKeyPressedThisFrame { get; }
        
        bool TryGetGamepad(out Gamepad gamepad);
    }
    
}