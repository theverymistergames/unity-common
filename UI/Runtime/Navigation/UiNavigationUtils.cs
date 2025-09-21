using System;
using MisterGames.Common.Maths;
using Unity.Burst;
using Unity.Mathematics;

namespace MisterGames.UI.Navigation {
    
    public static class UiNavigationUtils {
    
        [BurstCompile]
        public static bool IsInDirection(UiNavigationMode mode, int2 direction, float2 origin, float2 position) {
            return mode switch {
                UiNavigationMode.Grid => VectorUtils.Angle(direction, position - origin) <= 45f,
                
                UiNavigationMode.Vertical => direction switch {
                    { x: 0, y: 1 } => position.y > origin.y,
                    { x: 0, y: -1 } => position.y < origin.y,
                    _ => false,
                },
                    
                UiNavigationMode.Horizontal => direction switch {
                    { x: 1, y: 0 } => position.x > origin.x,
                    { x: -1, y: 0 } => position.x < origin.x,
                    _ => false,
                },
                    
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
    }
    
}