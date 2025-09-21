using System;
using System.Runtime.CompilerServices;
using MisterGames.Common.Maths;
using Unity.Burst;
using Unity.Mathematics;

namespace MisterGames.UI.Navigation {
    
    public static class UiNavigationUtils {
    
        private static readonly int2 Up = new(0, 1);
        private static readonly int2 Down = new(0, -1);
        private static readonly int2 Left = new(-1, 0);
        private static readonly int2 Right = new(1, 0);

        [BurstCompile]
        public static bool IsHigherThan(this float2 position, float2 relativeTo, UiNavigationMode mode) {
            return IsInDirection(position, relativeTo, Up, mode);
        }
        
        [BurstCompile]
        public static bool IsLowerThan(this float2 position, float2 relativeTo, UiNavigationMode mode) {
            return IsInDirection(position, relativeTo, Down, mode);
        }
        
        [BurstCompile]
        public static bool IsToTheLeftTo(this float2 position, float2 relativeTo, UiNavigationMode mode) {
            return IsInDirection(position, relativeTo, Left, mode);
        }
        
        [BurstCompile]
        public static bool IsToTheRightTo(this float2 position, float2 relativeTo, UiNavigationMode mode) {
            return IsInDirection(position, relativeTo, Right, mode);
        }
        
        [BurstCompile]
        public static bool IsInDirection(this float2 position, float2 relativeTo, UiNavigationDirection direction, UiNavigationMode mode) {
            var dir = direction switch {
                UiNavigationDirection.Up => Up,
                UiNavigationDirection.Down => Down,
                UiNavigationDirection.Left => Left,
                UiNavigationDirection.Right => Right,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
            
            return IsInDirection(position, relativeTo, dir, mode);
        }
        
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
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsInDirection(this float2 position, float2 relativeTo, int2 direction, UiNavigationMode mode) {
            return mode switch {
                UiNavigationMode.Grid => VectorUtils.Angle(direction, position - relativeTo) <= 45f,
                
                UiNavigationMode.Vertical => direction switch {
                    { x: 0, y: 1 } => position.y > relativeTo.y,
                    { x: 0, y: -1 } => position.y < relativeTo.y,
                    _ => false,
                },
                    
                UiNavigationMode.Horizontal => direction switch {
                    { x: 1, y: 0 } => position.x > relativeTo.x,
                    { x: -1, y: 0 } => position.x < relativeTo.x,
                    _ => false,
                },
                    
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
    }
    
}