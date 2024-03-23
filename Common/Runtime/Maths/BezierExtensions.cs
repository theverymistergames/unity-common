using UnityEngine;

namespace MisterGames.Common.Maths {
    
    public static class BezierExtensions {
        
        public static Vector3 EvaluateBezier3Points(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            return (1f - t) * (1f - t) * p0 + 
                   2f * t * (1f - t) * p1 + 
                   t * t * p2;
        }
        
        public static Vector3 EvaluateBezier4Points(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            return (1f - t) * (1f - t) * (1f - t) * p0 + 
                   3f * t * (1f - t) * (1f - t) * p1 + 
                   3f * t * t * (1f - t) * p2 +
                   t * t * t * p3;
        }
    }
    
}