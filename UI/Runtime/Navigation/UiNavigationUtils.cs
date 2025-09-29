using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace MisterGames.UI.Navigation {
    
    public static class UiNavigationUtils {
    
        private static readonly int2 Up = new(0, 1);
        private static readonly int2 Down = new(0, -1);
        private static readonly int2 Left = new(-1, 0);
        private static readonly int2 Right = new(1, 0);

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 DeltaInCells(this float2 vector, float2 cellSize) {
            return new int2((int) math.floor(vector.x / cellSize.x), (int) math.floor(vector.y / cellSize.y));
        }
        
        [BurstCompile]
        public static bool IsHigherThan(this float2 position, float2 relativeTo, float2 cellSize) {
            return IsInDirection(position, relativeTo, Up, cellSize);
        }
        
        [BurstCompile]
        public static bool IsLowerThan(this float2 position, float2 relativeTo, float2 cellSize) {
            return IsInDirection(position, relativeTo, Down, cellSize);
        }
        
        [BurstCompile]
        public static bool IsToTheLeftTo(this float2 position, float2 relativeTo, float2 cellSize) {
            return IsInDirection(position, relativeTo, Left, cellSize);
        }
        
        [BurstCompile]
        public static bool IsToTheRightTo(this float2 position, float2 relativeTo, float2 cellSize) {
            return IsInDirection(position, relativeTo, Right, cellSize);
        }
        
        [BurstCompile]
        public static bool IsInDirection(this float2 position, float2 relativeTo, UiNavigationDirection direction, float2 cellSize) {
            var dir = direction switch {
                UiNavigationDirection.Up => Up,
                UiNavigationDirection.Down => Down,
                UiNavigationDirection.Left => Left,
                UiNavigationDirection.Right => Right,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
            
            return IsInDirection(position, relativeTo, dir, cellSize);
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsInDirection(this float2 position, float2 relativeTo, int2 direction, float2 cellSize) {
            var cells = DeltaInCells(position - relativeTo, cellSize);

            if (math.abs(cells.y) >= math.abs(cells.x)) {
                return direction switch {
                    { x: 0, y: > 0 } => cells.y > 0,
                    { x: 0, y: < 0 } => cells.y < 0,
                    _ => false,
                };
            }

            return direction switch {
                { x: > 0, y: 0 } => cells.x > 0,
                { x: < 0, y: 0 } => cells.x < 0,
                _ => false,
            };
        }
        
    }
    
}