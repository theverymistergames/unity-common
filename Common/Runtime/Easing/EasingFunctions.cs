/*
* Modified by PeDev 2020
* The original version is https://gist.github.com/cjddmut/d789b9eb78216998e95c
*/

/*
*
* Created by C.J. Kimberlin
*
* The MIT License (MIT)
*
* Copyright (c) 2019
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
*
* TERMS OF USE - EASING EQUATIONS
* Open source under the BSD License.
* Copyright (c)2001 Robert Penner
* All rights reserved.
* Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
* Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
* THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
* FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*
*
* ============= Description =============
*
* Below is an example of how to use the easing functions in the file. There is a getting function that will return the function
* from an enum. This is useful since the enum can be exposed in the editor and then the function queried during Start().
*
* EasingFunction.Ease ease = EasingFunction.Ease.EaseInOutQuad;
* EasingFunction.EasingFunc func = GetEasingFunction(ease;
*
* float value = func(0, 10, 0.67f);
*
* EasingFunction.EasingFunc derivativeFunc = GetEasingFunctionDerivative(ease);
*
* float derivativeValue = derivativeFunc(0, 10, 0.67f);
*/

using System;
using UnityEngine;

namespace MisterGames.Common.Easing {

	public static class EasingFunctions {

        public static float Evaluate(this EasingType easingType, float start, float end, float value) {
            return easingType switch {
                EasingType.EaseInQuad => EaseInQuad(start, end, value),
                EasingType.EaseOutQuad => EaseOutQuad(start, end, value),
                EasingType.EaseInOutQuad => EaseInOutQuad(start, end, value),
                EasingType.EaseInCubic => EaseInCubic(start, end, value),
                EasingType.EaseOutCubic => EaseOutCubic(start, end, value),
                EasingType.EaseInOutCubic => EaseInOutCubic(start, end, value),
                EasingType.EaseInQuart => EaseInQuart(start, end, value),
                EasingType.EaseOutQuart => EaseOutQuart(start, end, value),
                EasingType.EaseInOutQuart => EaseInOutQuart(start, end, value),
                EasingType.EaseInQuint => EaseInQuint(start, end, value),
                EasingType.EaseOutQuint => EaseOutQuint(start, end, value),
                EasingType.EaseInOutQuint => EaseInOutQuint(start, end, value),
                EasingType.EaseInSine => EaseInSine(start, end, value),
                EasingType.EaseOutSine => EaseOutSine(start, end, value),
                EasingType.EaseInOutSine => EaseInOutSine(start, end, value),
                EasingType.EaseInExpo => EaseInExpo(start, end, value),
                EasingType.EaseOutExpo => EaseOutExpo(start, end, value),
                EasingType.EaseInOutExpo => EaseInOutExpo(start, end, value),
                EasingType.EaseInCirc => EaseInCirc(start, end, value),
                EasingType.EaseOutCirc => EaseOutCirc(start, end, value),
                EasingType.EaseInOutCirc => EaseInOutCirc(start, end, value),
                EasingType.Linear => Linear(start, end, value),
                EasingType.Spring => Spring(start, end, value),
                EasingType.EaseInBounce => EaseInBounce(start, end, value),
                EasingType.EaseOutBounce => EaseOutBounce(start, end, value),
                EasingType.EaseInOutBounce => EaseInOutBounce(start, end, value),
                EasingType.EaseInBack => EaseInBack(start, end, value),
                EasingType.EaseOutBack => EaseOutBack(start, end, value),
                EasingType.EaseInOutBack => EaseInOutBack(start, end, value),
                EasingType.EaseInElastic => EaseInElastic(start, end, value),
                EasingType.EaseOutElastic => EaseOutElastic(start, end, value),
                EasingType.EaseInOutElastic => EaseInOutElastic(start, end, value),
                EasingType.Constant0 => 0f,
                EasingType.Constant1 => 1f,
                _ => throw new NotImplementedException($"Easing function {easingType} is not implemented")
            };
        }

        public static float EvaluateDerivative(this EasingType easingType, float start, float end, float value) {
            return easingType switch {
                EasingType.EaseInQuad => EaseInQuadD(start, end, value),
                EasingType.EaseOutQuad => EaseOutQuadD(start, end, value),
                EasingType.EaseInOutQuad => EaseInOutQuadD(start, end, value),
                EasingType.EaseInCubic => EaseInCubicD(start, end, value),
                EasingType.EaseOutCubic => EaseOutCubicD(start, end, value),
                EasingType.EaseInOutCubic => EaseInOutCubicD(start, end, value),
                EasingType.EaseInQuart => EaseInQuartD(start, end, value),
                EasingType.EaseOutQuart => EaseOutQuartD(start, end, value),
                EasingType.EaseInOutQuart => EaseInOutQuartD(start, end, value),
                EasingType.EaseInQuint => EaseInQuintD(start, end, value),
                EasingType.EaseOutQuint => EaseOutQuintD(start, end, value),
                EasingType.EaseInOutQuint => EaseInOutQuintD(start, end, value),
                EasingType.EaseInSine => EaseInSineD(start, end, value),
                EasingType.EaseOutSine => EaseOutSineD(start, end, value),
                EasingType.EaseInOutSine => EaseInOutSineD(start, end, value),
                EasingType.EaseInExpo => EaseInExpoD(start, end, value),
                EasingType.EaseOutExpo => EaseOutExpoD(start, end, value),
                EasingType.EaseInOutExpo => EaseInOutExpoD(start, end, value),
                EasingType.EaseInCirc => EaseInCircD(start, end, value),
                EasingType.EaseOutCirc => EaseOutCircD(start, end, value),
                EasingType.EaseInOutCirc => EaseInOutCircD(start, end, value),
                EasingType.Linear => LinearD(start, end, value),
                EasingType.Spring => SpringD(start, end, value),
                EasingType.EaseInBounce => EaseInBounceD(start, end, value),
                EasingType.EaseOutBounce => EaseOutBounceD(start, end, value),
                EasingType.EaseInOutBounce => EaseInOutBounceD(start, end, value),
                EasingType.EaseInBack => EaseInBackD(start, end, value),
                EasingType.EaseOutBack => EaseOutBackD(start, end, value),
                EasingType.EaseInOutBack => EaseInOutBackD(start, end, value),
                EasingType.EaseInElastic => EaseInElasticD(start, end, value),
                EasingType.EaseOutElastic => EaseOutElasticD(start, end, value),
                EasingType.EaseInOutElastic => EaseInOutElasticD(start, end, value),
                EasingType.Constant0 => 0f,
                EasingType.Constant1 => 0f,
                _ => throw new NotImplementedException($"Easing function {easingType} is not implemented")
            };
        }

        private const float NATURAL_LOG_2 = 0.693147181f;

        private static float Linear(float start, float end, float value) {
            return Mathf.Lerp(start, end, value);
        }

        private static float Spring(float start, float end, float value) {
            value = Mathf.Clamp01(value);
            value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
            return start + (end - start) * value;
        }

        private static float EaseInQuad(float start, float end, float value) {
            end -= start;
            return end * value * value + start;
        }

        private static float EaseOutQuad(float start, float end, float value) {
            end -= start;
            return -end * value * (value - 2) + start;
        }

        private static float EaseInOutQuad(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value + start;
            value--;
            return -end * 0.5f * (value * (value - 2) - 1) + start;
        }

        private static float EaseInCubic(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value + start;
        }

        private static float EaseOutCubic(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value + 1) + start;
        }

        private static float EaseInOutCubic(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value + start;
            value -= 2;
            return end * 0.5f * (value * value * value + 2) + start;
        }

        private static float EaseInQuart(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value + start;
        }

        private static float EaseOutQuart(float start, float end, float value)
        {
            value--;
            end -= start;
            return -end * (value * value * value * value - 1) + start;
        }

        private static float EaseInOutQuart(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value * value + start;
            value -= 2;
            return -end * 0.5f * (value * value * value * value - 2) + start;
        }

        private static float EaseInQuint(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value * value + start;
        }

        private static float EaseOutQuint(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value * value * value + 1) + start;
        }

        private static float EaseInOutQuint(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value * value * value + start;
            value -= 2;
            return end * 0.5f * (value * value * value * value * value + 2) + start;
        }

        private static float EaseInSine(float start, float end, float value)
        {
            end -= start;
            return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
        }

        private static float EaseOutSine(float start, float end, float value)
        {
            end -= start;
            return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
        }

        private static float EaseInOutSine(float start, float end, float value)
        {
            end -= start;
            return -end * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1) + start;
        }

        private static float EaseInExpo(float start, float end, float value)
        {
            end -= start;
            return end * Mathf.Pow(2, 10 * (value - 1)) + start;
        }

        private static float EaseOutExpo(float start, float end, float value)
        {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * value) + 1) + start;
        }

        private static float EaseInOutExpo(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
            value--;
            return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
        }

        private static float EaseInCirc(float start, float end, float value)
        {
            end -= start;
            return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
        }

        private static float EaseOutCirc(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * Mathf.Sqrt(1 - value * value) + start;
        }

        private static float EaseInOutCirc(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return -end * 0.5f * (Mathf.Sqrt(1 - value * value) - 1) + start;
            value -= 2;
            return end * 0.5f * (Mathf.Sqrt(1 - value * value) + 1) + start;
        }

        private static float EaseInBounce(float start, float end, float value)
        {
            end -= start;
            const float d = 1f;
            return end - EaseOutBounce(0, end, d - value) + start;
        }

        private static float EaseOutBounce(float start, float end, float value)
        {
            value /= 1f;
            end -= start;
            if (value < (1 / 2.75f))
            {
                return end * (7.5625f * value * value) + start;
            }

            if (value < (2 / 2.75f))
            {
                value -= (1.5f / 2.75f);
                return end * (7.5625f * (value) * value + .75f) + start;
            }
            if (value < (2.5 / 2.75))
            {
                value -= (2.25f / 2.75f);
                return end * (7.5625f * (value) * value + .9375f) + start;
            }
            value -= (2.625f / 2.75f);
            return end * (7.5625f * (value) * value + .984375f) + start;
        }

        private static float EaseInOutBounce(float start, float end, float value)
        {
            end -= start;
            const float d = 1f;
            if (value < d * 0.5f) return EaseInBounce(0, end, value * 2) * 0.5f + start;
            return EaseOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
        }

        private static float EaseInBack(float start, float end, float value)
        {
            end -= start;
            value /= 1;
            const float s = 1.70158f;
            return end * (value) * value * ((s + 1) * value - s) + start;
        }

        private static float EaseOutBack(float start, float end, float value)
        {
            const float s = 1.70158f;
            end -= start;
            value -= 1;
            return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
        }

        private static float EaseInOutBack(float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value /= .5f;
            if ((value) < 1)
            {
                s *= (1.525f);
                return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
            }
            value -= 2;
            s *= (1.525f);
            return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
        }

        private static float EaseInElastic(float start, float end, float value)
        {
            end -= start;

            const float d = 1f;
            const float p = d * .3f;
            float s;
            float a = 0;

            if (Math.Abs(value) < float.Epsilon) return start;

            if (Math.Abs((value /= d) - 1) < float.Epsilon) return start + end;

            if (Math.Abs(a) < float.Epsilon || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        }

        private static float EaseOutElastic(float start, float end, float value)
        {
            end -= start;

            const float d = 1f;
            const float p = d * .3f;
            float s;
            float a = 0;

            if (Math.Abs(value) < float.Epsilon) return start;

            if (Math.Abs((value /= d) - 1) < float.Epsilon) return start + end;

            if (Math.Abs(a) < float.Epsilon || a < Mathf.Abs(end))
            {
                a = end;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
        }

        private static float EaseInOutElastic(float start, float end, float value)
        {
            end -= start;

            const float d = 1f;
            const float p = d * .3f;
            float s;
            float a = 0;

            if (Math.Abs(value) < float.Epsilon) return start;

            if (Math.Abs((value /= d * 0.5f) - 2) < float.Epsilon) return start + end;

            if (Math.Abs(a) < float.Epsilon || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
            return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
        }

        //
        // These are derived functions that the motor can use to get the speed at a specific time.
        //
        // The easing functions all work with a normalized time (0 to 1) and the returned value here
        // reflects that. Values returned here should be divided by the actual time.
        //
        // TODO: These functions have not had the testing they deserve. If there is odd behavior around
        //       dash speeds then this would be the first place I'd look.

        private static float LinearD(float start, float end, float value)
        {
            return end - start;
        }

        private static float EaseInQuadD(float start, float end, float value)
        {
            return 2f * (end - start) * value;
        }

        private static float EaseOutQuadD(float start, float end, float value)
        {
            end -= start;
            return -end * value - end * (value - 2);
        }

        private static float EaseInOutQuadD(float start, float end, float value)
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

        private static float EaseInCubicD(float start, float end, float value)
        {
            return 3f * (end - start) * value * value;
        }

        private static float EaseOutCubicD(float start, float end, float value)
        {
            value--;
            end -= start;
            return 3f * end * value * value;
        }

        private static float EaseInOutCubicD(float start, float end, float value)
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

        private static float EaseInQuartD(float start, float end, float value)
        {
            return 4f * (end - start) * value * value * value;
        }

        private static float EaseOutQuartD(float start, float end, float value)
        {
            value--;
            end -= start;
            return -4f * end * value * value * value;
        }

        private static float EaseInOutQuartD(float start, float end, float value)
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

        private static float EaseInQuintD(float start, float end, float value)
        {
            return 5f * (end - start) * value * value * value * value;
        }

        private static float EaseOutQuintD(float start, float end, float value)
        {
            value--;
            end -= start;
            return 5f * end * value * value * value * value;
        }

        private static float EaseInOutQuintD(float start, float end, float value)
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

        private static float EaseInSineD(float start, float end, float value)
        {
            return (end - start) * 0.5f * Mathf.PI * Mathf.Sin(0.5f * Mathf.PI * value);
        }

        private static float EaseOutSineD(float start, float end, float value)
        {
            end -= start;
            return (Mathf.PI * 0.5f) * end * Mathf.Cos(value * (Mathf.PI * 0.5f));
        }

        private static float EaseInOutSineD(float start, float end, float value)
        {
            end -= start;
            return end * 0.5f * Mathf.PI * Mathf.Sin(Mathf.PI * value);
        }

        private static float EaseInExpoD(float start, float end, float value)
        {
            return 10f * NATURAL_LOG_2 * (end - start) * Mathf.Pow(2f, 10f * (value - 1));
        }

        private static float EaseOutExpoD(float start, float end, float value)
        {
            end -= start;
            return 5f * NATURAL_LOG_2 * end * Mathf.Pow(2f, 1f - 10f * value);
        }

        private static float EaseInOutExpoD(float start, float end, float value)
        {
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return 5f * NATURAL_LOG_2 * end * Mathf.Pow(2f, 10f * (value - 1));
            }

            value--;

            return (5f * NATURAL_LOG_2 * end) / (Mathf.Pow(2f, 10f * value));
        }

        private static float EaseInCircD(float start, float end, float value)
        {
            return (end - start) * value / Mathf.Sqrt(1f - value * value);
        }

        private static float EaseOutCircD(float start, float end, float value)
        {
            value--;
            end -= start;
            return (-end * value) / Mathf.Sqrt(1f - value * value);
        }

        private static float EaseInOutCircD(float start, float end, float value)
        {
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return (end * value) / (2f * Mathf.Sqrt(1f - value * value));
            }

            value -= 2;

            return (-end * value) / (2f * Mathf.Sqrt(1f - value * value));
        }

        private static float EaseInBounceD(float start, float end, float value)
        {
            end -= start;
            const float d = 1f;

            return EaseOutBounceD(0, end, d - value);
        }

        private static float EaseOutBounceD(float start, float end, float value)
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

        private static float EaseInOutBounceD(float start, float end, float value)
        {
            end -= start;
            const float d = 1f;

            if (value < d * 0.5f)
            {
                return EaseInBounceD(0, end, value * 2) * 0.5f;
            }

            return EaseOutBounceD(0, end, value * 2 - d) * 0.5f;
        }

        private static float EaseInBackD(float start, float end, float value)
        {
            const float s = 1.70158f;

            return 3f * (s + 1f) * (end - start) * value * value - 2f * s * (end - start) * value;
        }

        private static float EaseOutBackD(float start, float end, float value)
        {
            const float s = 1.70158f;
            end -= start;
            value -= 1;

            return end * ((s + 1f) * value * value + 2f * value * ((s + 1f) * value + s));
        }

        private static float EaseInOutBackD(float start, float end, float value)
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

        private static float EaseInElasticD(float start, float end, float value)
        {
            return EaseOutElasticD(start, end, 1f - value);
        }

        private static float EaseOutElasticD(float start, float end, float value)
        {
            end -= start;

            const float d = 1f;
            const float p = d * .3f;
            float s;
            float a = 0;

            if (Math.Abs(a) < float.Epsilon || a < Mathf.Abs(end))
            {
                a = end;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.PI * d * Mathf.Pow(2f, 1f - 10f * value) *
                    Mathf.Cos((2f * Mathf.PI * (d * value - s)) / p)) / p - 5f * NATURAL_LOG_2 * a *
                Mathf.Pow(2f, 1f - 10f * value) * Mathf.Sin((2f * Mathf.PI * (d * value - s)) / p);
        }

        private static float EaseInOutElasticD(float start, float end, float value)
        {
            end -= start;

            const float d = 1f;
            const float p = d * .3f;
            float s;
            float a = 0;

            if (Math.Abs(a) < float.Epsilon || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (value < 1)
            {
                value -= 1;

                return -5f * NATURAL_LOG_2 * a * Mathf.Pow(2f, 10f * value) * Mathf.Sin(2 * Mathf.PI * (d * value - 2f) / p) -
                       a * Mathf.PI * d * Mathf.Pow(2f, 10f * value) * Mathf.Cos(2 * Mathf.PI * (d * value - s) / p) / p;
            }

            value -= 1;

            return a * Mathf.PI * d * Mathf.Cos(2f * Mathf.PI * (d * value - s) / p) / (p * Mathf.Pow(2f, 10f * value)) -
                   5f * NATURAL_LOG_2 * a * Mathf.Sin(2f * Mathf.PI * (d * value - s) / p) / (Mathf.Pow(2f, 10f * value));
        }

        private static float SpringD(float start, float end, float value)
        {
            value = Mathf.Clamp01(value);
            end -= start;

            // Damn... Thanks http://www.derivative-calculator.net/
            // TODO: And it's a little bit wrong
            return end * (6f * (1f - value) / 5f + 1f) * (-2.2f * Mathf.Pow(1f - value, 1.2f) *
                       Mathf.Sin(Mathf.PI * value * (2.5f * value * value * value + 0.2f)) + Mathf.Pow(1f - value, 2.2f) *
                       (Mathf.PI * (2.5f * value * value * value + 0.2f) + 7.5f * Mathf.PI * value * value * value) *
                       Mathf.Cos(Mathf.PI * value * (2.5f * value * value * value + 0.2f)) + 1f) -
                   6f * end * (Mathf.Pow(1 - value, 2.2f) * Mathf.Sin(Mathf.PI * value * (2.5f * value * value * value + 0.2f)) + value
                       / 5f);

        }
	}

}
