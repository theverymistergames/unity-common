using System;
using UnityEngine;

namespace MisterGames.Input.Bindings {
    
    public interface IInputBindingHelper {
    
        bool IsKeyPressed(KeyBinding key);

        bool WasKeyPressedThisFrame(KeyBinding key);

        bool WasKeyReleasedThisFrame(KeyBinding key);

        Vector2 GetAxisValue(AxisBinding axis);

        bool AreModifiersPressed(ShortcutModifiers key);
        
        void AddKeyPressCallback(KeyBinding keyBinding, Action callback);
        
        void RemoveKeyPressCallback(KeyBinding keyBinding, Action callback);
        
        void AddKeyReleaseCallback(KeyBinding keyBinding, Action callback);
        
        void RemoveKeyReleaseCallback(KeyBinding keyBinding, Action callback);
    }
    
}