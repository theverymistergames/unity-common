using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace MisterGames.Common.Easing {

	public static class EasingFunctions {

        private const float aExpo = 1.0009775171065494f;
        private const float bExpo = -0.0009775171065494f;
        private const float bExpo2 = -0.0004887585532747f;
        private const float sBounce = 1f / 2.75f;
        private const float sBack = 1.70158f;
        private const float sBack1525 = sBack * 1.525f;
        private const float sElastic = math.PI2 / 0.3f;
            
        [BurstCompile]
        public static float Evaluate(this EasingType easingType, float value) {
            return easingType switch {
                EasingType.EaseInQuad => EaseInQuad(value),
                EasingType.EaseOutQuad => EaseOutQuad(value),
                EasingType.EaseInOutQuad => EaseInOutQuad(value),
                EasingType.EaseInCubic => EaseInCubic(value),
                EasingType.EaseOutCubic => EaseOutCubic(value),
                EasingType.EaseInOutCubic => EaseInOutCubic(value),
                EasingType.EaseInQuart => EaseInQuart(value),
                EasingType.EaseOutQuart => EaseOutQuart(value),
                EasingType.EaseInOutQuart => EaseInOutQuart(value),
                EasingType.EaseInQuint => EaseInQuint(value),
                EasingType.EaseOutQuint => EaseOutQuint(value),
                EasingType.EaseInOutQuint => EaseInOutQuint(value),
                EasingType.EaseInSine => EaseInSine(value),
                EasingType.EaseOutSine => EaseOutSine(value),
                EasingType.EaseInOutSine => EaseInOutSine(value),
                EasingType.EaseInExpo => EaseInExpo(value),
                EasingType.EaseOutExpo => EaseOutExpo(value),
                EasingType.EaseInOutExpo => EaseInOutExpo(value),
                EasingType.EaseInCirc => EaseInCirc(value),
                EasingType.EaseOutCirc => EaseOutCirc(value),
                EasingType.EaseInOutCirc => EaseInOutCirc(value),
                EasingType.Linear => Linear(value),
                EasingType.Spring => Spring(value),
                EasingType.EaseInBounce => EaseInBounce(value),
                EasingType.EaseOutBounce => EaseOutBounce(value),
                EasingType.EaseInOutBounce => EaseInOutBounce(value),
                EasingType.EaseInBack => EaseInBack(value),
                EasingType.EaseOutBack => EaseOutBack(value),
                EasingType.EaseInOutBack => EaseInOutBack(value),
                EasingType.EaseInElastic => EaseInElastic(value),
                EasingType.EaseOutElastic => EaseOutElastic(value),
                EasingType.EaseInOutElastic => EaseInOutElastic(value),
                EasingType.Constant0 => 0f,
                EasingType.Constant1 => 1f,
                _ => throw new NotImplementedException($"Easing function {easingType} is not implemented")
            };
        }
/*
        [BurstCompile]
        public static float EvaluateDerivative(this EasingType easingType, float value) {
            return easingType switch {
                EasingType.EaseInQuad => EaseInQuadD(value),
                EasingType.EaseOutQuad => EaseOutQuadD(value),
                EasingType.EaseInOutQuad => EaseInOutQuadD(value),
                EasingType.EaseInCubic => EaseInCubicD(value),
                EasingType.EaseOutCubic => EaseOutCubicD(value),
                EasingType.EaseInOutCubic => EaseInOutCubicD(value),
                EasingType.EaseInQuart => EaseInQuartD(value),
                EasingType.EaseOutQuart => EaseOutQuartD(value),
                EasingType.EaseInOutQuart => EaseInOutQuartD(value),
                EasingType.EaseInQuint => EaseInQuintD(value),
                EasingType.EaseOutQuint => EaseOutQuintD(value),
                EasingType.EaseInOutQuint => EaseInOutQuintD(value),
                EasingType.EaseInSine => EaseInSineD(value),
                EasingType.EaseOutSine => EaseOutSineD(value),
                EasingType.EaseInOutSine => EaseInOutSineD(value),
                EasingType.EaseInExpo => EaseInExpoD(value),
                EasingType.EaseOutExpo => EaseOutExpoD(value),
                EasingType.EaseInOutExpo => EaseInOutExpoD(value),
                EasingType.EaseInCirc => EaseInCircD(value),
                EasingType.EaseOutCirc => EaseOutCircD(value),
                EasingType.EaseInOutCirc => EaseInOutCircD(value),
                EasingType.Linear => LinearD(value),
                EasingType.Spring => SpringD(value),
                EasingType.EaseInBounce => EaseInBounceD(value),
                EasingType.EaseOutBounce => EaseOutBounceD(value),
                EasingType.EaseInOutBounce => EaseInOutBounceD(value),
                EasingType.EaseInBack => EaseInBackD(value),
                EasingType.EaseOutBack => EaseOutBackD(value),
                EasingType.EaseInOutBack => EaseInOutBackD(value),
                EasingType.EaseInElastic => EaseInElasticD(value),
                EasingType.EaseOutElastic => EaseOutElasticD(value),
                EasingType.EaseInOutElastic => EaseInOutElasticD(value),
                EasingType.Constant0 => 0f,
                EasingType.Constant1 => 0f,
                _ => throw new NotImplementedException($"Easing function {easingType} is not implemented")
            };
        }
*/
        private const float NATURAL_LOG_2 = 0.693147181f;

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Linear(float value) {
            return value;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Spring(float value) {
            return (math.sin(value * math.PI * (0.2f + 2.5f * value * value * value)) * math.pow(1f - value, 2.2f) + value) * 
                   (1f + 1.2f * (1f - value));
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInQuad(float value) {
            return value * value;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutQuad(float value) {
            return value * (2f - value);
        }
        
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutQuad(float value) {
            return value < 0.5f 
                ? 2f * value * value 
                : -2f * value * value + 4f * value - 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInCubic(float value) {
            return value * value * value;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutCubic(float value) {
            return (value - 1f) * (value - 1f) * (value - 1f) + 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutCubic(float value) {
            return value < 0.5f 
                ? 4f * value * value * value 
                : 4f * (value - 1f) * (value - 1f) * (value - 1f) + 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInQuart(float value) {
            return value * value * value * value;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutQuart(float value) {
            return 1f - (value - 1f) * (value - 1f) * (value - 1f) * (value - 1f);
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutQuart(float value) {
            return value < 0.5f 
                ? 8f * value * value * value * value 
                : -8f * (value - 1f) * (value - 1f) * (value - 1f) * (value - 1f) + 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInQuint(float value) {
            return value * value * value * value * value;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutQuint(float value) {
            return (value - 1f) * (value - 1f) * (value - 1f) * (value - 1f) * (value - 1f) + 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutQuint(float value) {
            return value < 0.5f 
                ? 16f * value * value * value * value * value 
                : 16f * (value - 1) * (value - 1) * (value - 1) * (value - 1) * (value - 1) + 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInSine(float value) {
            return -1f * math.cos(value * (math.PI * 0.5f)) + 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutSine(float value) {
            return math.sin(value * (math.PI * 0.5f));
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutSine(float value) {
            return -0.5f * (math.cos(math.PI * value) - 1f);
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInExpo(float value) {
            // 2^-10 is not zero, so we need to scale to match x,y = 0 and x,y = 1 
            return aExpo * math.pow(2f, 10f * (value - 1f)) + bExpo;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutExpo(float value) {
            return -aExpo * math.pow(2f, -10f * value) + aExpo;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutExpo(float value) {
            return value < 0.5f 
                ? aExpo * 0.5f * math.pow(2f, 10f * (2f * value - 1f)) + bExpo2
                : aExpo * -0.5f * math.pow(2f, -10f * (2f * value - 1f)) + 1f - bExpo2;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInCirc(float value) {
            return -math.sqrt(1f - value * value) + 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutCirc(float value) {
            return math.sqrt(1f - (value - 1f) * (value - 1f));
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutCirc(float value) {
            return value < 0.5f 
                ? -0.5f * math.sqrt(1f - 4f * value * value) + 0.5f 
                : 0.5f * math.sqrt(1f - 4f * (value - 1f) * (value - 1f)) + 0.5f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInBounce(float value) {
            return 1f - EaseOutBounce(1f - value);
        }
        
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutBounce(float value) {
            return value switch {
                < sBounce => 7.5625f * value * value,
                < 2f * sBounce => 7.5625f * (value - 1.5f * sBounce) * (value - 1.5f * sBounce) + 0.75f,
                < 2.5f * sBounce => 7.5625f * (value - 2.25f * sBounce) * (value - 2.25f * sBounce) + 0.9375f,
                _ => 7.5625f * (value - 2.625f * sBounce) * (value - 2.625f * sBounce) + .984375f
            };
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutBounce(float value) {
            return value < 0.5f 
                ? EaseInBounce(value * 2f) * 0.5f 
                : EaseOutBounce(value * 2f - 1f) * 0.5f + 0.5f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInBack(float value) {
            return value * value * ((sBack + 1) * value - sBack);
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutBack(float value) {
            return (value - 1f) * (value - 1f) * ((sBack + 1) * (value - 1f) + sBack) + 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutBack(float value) {
            return value < 0.5f
                ? 0.5f * (4f * value * value * ((sBack1525 + 1f) * 2f * value - sBack1525)) 
                : 2f * (value - 1f) * (value - 1f) * ((sBack1525 + 1f) * 2f * (value - 1f) + sBack1525) + 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInElastic(float value) {
            if (1f - 4 * (value - 0.5f) * (value - 0.5f) < float.Epsilon) return value;
            return -math.pow(2f, 10f * (value - 1f)) * math.sin((value - 1.075f) * sElastic);
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseOutElastic(float value) {
            if (1f - 4 * (value - 0.5f) * (value - 0.5f) < float.Epsilon) return value;
            return math.pow(2f, -10f * value) * math.sin((value - 0.075f) * sElastic) + 1f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EaseInOutElastic(float value) {
            if (1f - 4f * (value - 0.5f) * (value - 0.5f) < float.Epsilon) return value;
            
            return value < 0.5f
                ? -0.5f * math.pow(2f, 20f * value - 10f) * math.sin((2f * value - 1.075f) * sElastic) 
                : 0.5f * math.pow(2f, -20f * value + 10f) * math.sin((2f * value - 1.075f) * sElastic) + 1f;
        }
        
        //
        // These are derived functions that the motor can use to get the speed at a specific time.
        //
        // The easing functions all work with a normalized time (0 to 1) and the returned value here
        // reflects that. Values returned here should be divided by the actual time.
        //
        // TODO: These functions have not had the testing they deserve. If there is odd behavior around
        //       dash speeds then this would be the first place I'd look.

        /*
        private static float LinearD(float value)
        {
            return end - start;
        }

        private static float EaseInQuadD(float value)
        {
            return 2f * (end - start) * value;
        }

        private static float EaseOutQuadD(float value)
        {
            end -= start;
            return -end * value - end * (value - 2);
        }

        private static float EaseInOutQuadD(float value)
        {
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return end * value;
            }

            value--;

            return end * (1 - value);
        }

        private static float EaseInCubicD(float value)
        {
            return 3f * (end - start) * value * value;
        }

        private static float EaseOutCubicD(float value)
        {
            value--;
            end -= start;
            return 3f * end * value * value;
        }

        private static float EaseInOutCubicD(float value)
        {
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return (3f / 2f) * end * value * value;
            }

            value -= 2;

            return (3f / 2f) * end * value * value;
        }

        private static float EaseInQuartD(float value)
        {
            return 4f * (end - start) * value * value * value;
        }

        private static float EaseOutQuartD(float value)
        {
            value--;
            end -= start;
            return -4f * end * value * value * value;
        }

        private static float EaseInOutQuartD(float value)
        {
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return 2f * end * value * value * value;
            }

            value -= 2;

            return -2f * end * value * value * value;
        }

        private static float EaseInQuintD(float value)
        {
            return 5f * (end - start) * value * value * value * value;
        }

        private static float EaseOutQuintD(float value)
        {
            value--;
            end -= start;
            return 5f * end * value * value * value * value;
        }

        private static float EaseInOutQuintD(float value)
        {
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return (5f / 2f) * end * value * value * value * value;
            }

            value -= 2;

            return (5f / 2f) * end * value * value * value * value;
        }

        private static float EaseInSineD(float value)
        {
            return (end - start) * 0.5f * math.PI * math.Sin(0.5f * math.PI * value);
        }

        private static float EaseOutSineD(float value)
        {
            end -= start;
            return (math.PI * 0.5f) * end * math.Cos(value * (math.PI * 0.5f));
        }

        private static float EaseInOutSineD(float value)
        {
            end -= start;
            return end * 0.5f * math.PI * math.Sin(math.PI * value);
        }

        private static float EaseInExpoD(float value)
        {
            return 10f * NATURAL_LOG_2 * (end - start) * math.Pow(2f, 10f * (value - 1));
        }

        private static float EaseOutExpoD(float value)
        {
            end -= start;
            return 5f * NATURAL_LOG_2 * end * math.Pow(2f, 1f - 10f * value);
        }

        private static float EaseInOutExpoD(float value)
        {
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return 5f * NATURAL_LOG_2 * end * math.Pow(2f, 10f * (value - 1));
            }

            value--;

            return (5f * NATURAL_LOG_2 * end) / (math.Pow(2f, 10f * value));
        }

        private static float EaseInCircD(float value)
        {
            return (end - start) * value / math.Sqrt(1f - value * value);
        }

        private static float EaseOutCircD(float value)
        {
            value--;
            end -= start;
            return (-end * value) / math.Sqrt(1f - value * value);
        }

        private static float EaseInOutCircD(float value)
        {
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return (end * value) / (2f * math.Sqrt(1f - value * value));
            }

            value -= 2;

            return (-end * value) / (2f * math.Sqrt(1f - value * value));
        }

        private static float EaseInBounceD(float value)
        {
            end -= start;
            const float d = 1f;

            return EaseOutBounceD(0, end, d - value);
        }

        private static float EaseOutBounceD(float value)
        {
            value /= 1f;
            end -= start;

            if (value < (1 / 2.75f))
            {
                return 2f * end * 7.5625f * value;
            }

            if (value < (2 / 2.75f))
            {
                value -= (1.5f / 2.75f);
                return 2f * end * 7.5625f * value;
            }
            if (value < (2.5 / 2.75))
            {
                value -= (2.25f / 2.75f);
                return 2f * end * 7.5625f * value;
            }
            value -= (2.625f / 2.75f);
            return 2f * end * 7.5625f * value;
        }

        private static float EaseInOutBounceD(float value)
        {
            end -= start;
            const float d = 1f;

            if (value < d * 0.5f)
            {
                return EaseInBounceD(0, end, value * 2) * 0.5f;
            }

            return EaseOutBounceD(0, end, value * 2 - d) * 0.5f;
        }

        private static float EaseInBackD(float value)
        {
            const float s = 1.70158f;

            return 3f * (s + 1f) * (end - start) * value * value - 2f * s * (end - start) * value;
        }

        private static float EaseOutBackD(float value)
        {
            const float s = 1.70158f;
            end -= start;
            value -= 1;

            return end * ((s + 1f) * value * value + 2f * value * ((s + 1f) * value + s));
        }

        private static float EaseInOutBackD(float value)
        {
            float s = 1.70158f;
            end -= start;
            value /= .5f;

            if ((value) < 1)
            {
                s *= (1.525f);
                return 0.5f * end * (s + 1) * value * value + end * value * ((s + 1f) * value - s);
            }

            value -= 2;
            s *= (1.525f);
            return 0.5f * end * ((s + 1) * value * value + 2f * value * ((s + 1f) * value + s));
        }

        private static float EaseInElasticD(float value)
        {
            return EaseOutElasticD(start, end, 1f - value);
        }

        private static float EaseOutElasticD(float value)
        {
            end -= start;

            const float d = 1f;
            const float p = d * .3f;
            float s;
            float a = 0;

            if (Math.Abs(a) < float.Epsilon || a < math.Abs(end))
            {
                a = end;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * math.PI) * math.Asin(end / a);
            }

            return (a * math.PI * d * math.Pow(2f, 1f - 10f * value) *
                    math.Cos((2f * math.PI * (d * value - s)) / p)) / p - 5f * NATURAL_LOG_2 * a *
                math.Pow(2f, 1f - 10f * value) * math.Sin((2f * math.PI * (d * value - s)) / p);
        }

        private static float EaseInOutElasticD(float value)
        {
            end -= start;

            const float d = 1f;
            const float p = d * .3f;
            float s;
            float a = 0;

            if (Math.Abs(a) < float.Epsilon || a < math.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * math.PI) * math.Asin(end / a);
            }

            if (value < 1)
            {
                value -= 1;

                return -5f * NATURAL_LOG_2 * a * math.Pow(2f, 10f * value) * math.Sin(2 * math.PI * (d * value - 2f) / p) -
                       a * math.PI * d * math.Pow(2f, 10f * value) * math.Cos(2 * math.PI * (d * value - s) / p) / p;
            }

            value -= 1;

            return a * math.PI * d * math.Cos(2f * math.PI * (d * value - s) / p) / (p * math.Pow(2f, 10f * value)) -
                   5f * NATURAL_LOG_2 * a * math.Sin(2f * math.PI * (d * value - s) / p) / (math.Pow(2f, 10f * value));
        }

        private static float SpringD(float value)
        {
            value = math.Clamp01(value);
            end -= start;

            // Damn... Thanks http://www.derivative-calculator.net/
            // TODO: And it's a little bit wrong
            return end * (6f * (1f - value) / 5f + 1f) * (-2.2f * math.Pow(1f - value, 1.2f) *
                       math.Sin(math.PI * value * (2.5f * value * value * value + 0.2f)) + math.Pow(1f - value, 2.2f) *
                       (math.PI * (2.5f * value * value * value + 0.2f) + 7.5f * math.PI * value * value * value) *
                       math.Cos(math.PI * value * (2.5f * value * value * value + 0.2f)) + 1f) -
                   6f * end * (math.Pow(1 - value, 2.2f) * math.Sin(math.PI * value * (2.5f * value * value * value + 0.2f)) + value
                       / 5f);

        }
        */
	}

}
