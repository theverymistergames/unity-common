using System;
using System.Runtime.CompilerServices;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.Input.Bindings {
    
    public static class InputBindingExtensions {
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPressed(this KeyBinding key) {
            return InputServices.BindingHelper?.IsKeyPressed(key) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WasPressedThisFrame(this KeyBinding key) {
            return InputServices.BindingHelper?.WasKeyPressedThisFrame(key) ?? false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WasReleasedThisFrame(this KeyBinding key) {
            return InputServices.BindingHelper?.WasKeyReleasedThisFrame(key) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetVector(this AxisBinding axis) {
            return InputServices.BindingHelper?.GetAxisValue(axis) ?? default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ArePressed(this ShortcutModifiers key) {
            return InputServices.BindingHelper?.AreModifiersPressed(key) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPressCallback(this KeyBinding key, Action callback) {
            InputServices.BindingHelper?.AddKeyPressCallback(key, callback);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemovePressCallback(this KeyBinding key, Action callback) {
            InputServices.BindingHelper?.RemoveKeyPressCallback(key, callback);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddReleaseCallback(this KeyBinding key, Action callback) {
            InputServices.BindingHelper?.AddKeyReleaseCallback(key, callback);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveReleaseCallback(this KeyBinding key, Action callback) {
            InputServices.BindingHelper?.RemoveKeyReleaseCallback(key, callback);
        }
    }
    
}