using UnityEngine;

namespace MisterGames.Common.Maths {

    public static class AnimationCurves {

        public static AnimationCurve Arc(
            float timeStart,
            float valueStart,
            float tangentStart,
            float timeEnd,
            float valueEnd,
            float tangentEnd
        ) {
            if (timeStart.IsNearlyEqual(timeEnd)) {
                return new AnimationCurve(new Keyframe(timeStart, valueStart));
            }

            return new AnimationCurve(
                new Keyframe(timeStart, valueStart, 0.0f, tangentStart), 
                new Keyframe(timeEnd, valueEnd, tangentEnd, 0.0f)
            );
        }

        public static AnimationCurve Arc(
            float timeStart,
            float valueStart,
            float tangentStart,
            float timeMid,
            float valueMid,
            float inTangentMid,
            float outTangentMid,
            float timeEnd,
            float valueEnd,
            float tangentEnd
        ) {
            if (timeStart.IsNearlyEqual(timeEnd)) {
                return new AnimationCurve(new Keyframe(timeStart, valueStart));
            }

            return new AnimationCurve(
                new Keyframe(timeStart, valueStart, 0.0f, tangentStart), 
                new Keyframe(timeMid, valueMid, inTangentMid, outTangentMid), 
                new Keyframe(timeEnd, valueEnd, tangentEnd, 0.0f)
            );
        }
        
    }

}