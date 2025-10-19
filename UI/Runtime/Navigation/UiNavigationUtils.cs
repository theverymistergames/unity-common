using System;
using System.Runtime.CompilerServices;
using MisterGames.Common.Service;
using MisterGames.UI.Service;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.UI.Navigation {
    
    public static class UiNavigationUtils {
    
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHigherThan(this float2 position, float2 relativeTo) => position.y > relativeTo.y;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLowerThan(this float2 position, float2 relativeTo) => position.y < relativeTo.y;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsToTheLeftTo(this float2 position, float2 relativeTo) => position.x < relativeTo.x;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsToTheRightTo(this float2 position, float2 relativeTo) => position.x > relativeTo.x;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInDirection(this float2 position, float2 relativeTo, UiNavigationDirection direction) {
            return direction switch {
                UiNavigationDirection.Up => position.y > relativeTo.y,
                UiNavigationDirection.Down => position.y < relativeTo.y,
                UiNavigationDirection.Left => position.x < relativeTo.x,
                UiNavigationDirection.Right => position.x > relativeTo.x,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        public static bool IsCursorInsideRect(RectTransform rectTransform) {
            var camera = Services.TryGet(out CanvasRegistry canvasRegistry) &&
                         canvasRegistry.TryGetCurrentEventCamera(out var c)
                ? c
                : null;

            return Cursor.visible &&
                   RectTransformUtility.RectangleContainsScreenPoint(rectTransform, UnityEngine.Input.mousePosition, camera);
        }
    }
    
}