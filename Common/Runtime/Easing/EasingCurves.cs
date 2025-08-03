using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Easing {

	public static class EasingCurves {

		public static AnimationCurve ToAnimationCurve(this EasingType ease) {
			return new AnimationCurve { keys = GetEasingTypeKeys(ease) };
		}

		public static void InvertAnimationCurveX(this AnimationCurve curve) {
			if (curve.keys is not { Length: > 1 } keys) return;

			int length = keys.Length;
			int halfLength = Mathf.CeilToInt(length * 0.5f);

			float startTime = keys[0].time;
			float endTime = keys[^1].time;
			
			for (int i = 0; i < halfLength; i++) {
				
				var ka = keys[i];
				var kb = keys[length - 1 - i];
				
				keys[i] = new Keyframe(startTime + endTime - kb.time, kb.value, -kb.outTangent, -kb.inTangent, kb.outWeight, kb.inWeight) { weightedMode = kb.weightedMode };
				keys[length - 1 - i] = new Keyframe(startTime + endTime - ka.time, ka.value, -ka.outTangent, -ka.inTangent, ka.outWeight, ka.inWeight) { weightedMode = ka.weightedMode };
			}

			curve.keys = keys;
		}
		
		public static void InvertAnimationCurveY(this AnimationCurve curve) {
			if (curve.keys is not { Length: > 1 } keys) return;

			int length = keys.Length;
			
			float minValue = float.MaxValue;
			float maxValue = float.MinValue;
			
			for (int i = 0; i < length; i++) {
				var k = keys[i];
				if (k.value > maxValue) maxValue = k.value;
				if (k.value < minValue) minValue = k.value;
			}
			
			for (int i = 0; i < length; i++) {
				var k = keys[i];
				keys[i] = new Keyframe(k.time, minValue + maxValue - k.value, -k.inTangent, -k.outTangent, k.inWeight, k.outWeight) { weightedMode = k.weightedMode };
			}

			curve.keys = keys;
		}

		public static bool TryGetEasingType(this AnimationCurve animationCurve, out EasingType easingType) {
			if (animationCurve == null) {
				easingType = default;
				return false;
			}

			var keys = animationCurve.keys;
			var types = Enum.GetValues(typeof(EasingType));

			foreach (EasingType type in types) {
				var easingTypeKeys = GetEasingTypeKeys(type);
				if (easingTypeKeys.Length != keys.Length) continue;

				bool hasSameKeys = true;

				for (int i = 0; i < keys.Length; i++) {
					var key = keys[i];
					var easingTypeKey = easingTypeKeys[i];

					if (key.weightedMode != easingTypeKey.weightedMode ||
					    Math.Abs(key.time - easingTypeKey.time) > Mathf.Epsilon ||
					    Math.Abs(key.value - easingTypeKey.value) > Mathf.Epsilon ||
					    Math.Abs(key.inTangent - easingTypeKey.inTangent) > Mathf.Epsilon ||
					    Math.Abs(key.outTangent - easingTypeKey.outTangent) > Mathf.Epsilon ||
					    Math.Abs(key.inWeight - easingTypeKey.inWeight) > Mathf.Epsilon ||
					    Math.Abs(key.outWeight - easingTypeKey.outWeight) > Mathf.Epsilon
					) {
						hasSameKeys = false;
						break;
					}
				}

				if (!hasSameKeys) continue;

				easingType = type;
				return true;
			}

			easingType = default;
			return false;
		}

		private static Keyframe[] GetEasingTypeKeys(EasingType ease) {
			return ease switch {
				EasingType.EaseInQuad => EaseInQuad.keys,
				EasingType.EaseOutQuad => EaseOutQuad.keys,
				EasingType.EaseInOutQuad => EaseInOutQuad.keys,
				EasingType.EaseInCubic => EaseInCubic.keys,
				EasingType.EaseOutCubic => EaseOutCubic.keys,
				EasingType.EaseInOutCubic => EaseInOutCubic.keys,
				EasingType.EaseInQuart => EaseInQuart.keys,
				EasingType.EaseOutQuart => EaseOutQuart.keys,
				EasingType.EaseInOutQuart => EaseInOutQuart.keys,
				EasingType.EaseInQuint => EaseInQuint.keys,
				EasingType.EaseOutQuint => EaseOutQuint.keys,
				EasingType.EaseInOutQuint => EaseInOutQuint.keys,
				EasingType.EaseInSine => EaseInSine.keys,
				EasingType.EaseOutSine => EaseOutSine.keys,
				EasingType.EaseInOutSine => EaseInOutSine.keys,
				EasingType.EaseInExpo => EaseInExpo.keys,
				EasingType.EaseOutExpo => EaseOutExpo.keys,
				EasingType.EaseInOutExpo => EaseInOutExpo.keys,
				EasingType.EaseInCirc => EaseInCirc.keys,
				EasingType.EaseOutCirc => EaseOutCirc.keys,
				EasingType.EaseInOutCirc => EaseInOutCirc.keys,
				EasingType.Linear => Linear.keys,
				EasingType.Spring => Spring.keys,
				EasingType.EaseInBounce => EaseInBounce.keys,
				EasingType.EaseOutBounce => EaseOutBounce.keys,
				EasingType.EaseInOutBounce => EaseInOutBounce.keys,
				EasingType.EaseInBack => EaseInBack.keys,
				EasingType.EaseOutBack => EaseOutBack.keys,
				EasingType.EaseInOutBack => EaseInOutBack.keys,
				EasingType.EaseInElastic => EaseInElastic.keys,
				EasingType.EaseOutElastic => EaseOutElastic.keys,
				EasingType.EaseInOutElastic => EaseInOutElastic.keys,
				EasingType.Constant0 => Constant0.keys,
				EasingType.Constant1 => Constant1.keys,
				_ => throw new NotImplementedException($"Easing animation curve is not implemented for easing function {ease}")
			};
		}

		private static AnimationCurve BezierToAnimationCurve(IReadOnlyList<Vector2> controlPointStrips) {
			if (controlPointStrips.Count < 4) {
				throw new ArgumentException("The number of control point strips should more than 4!");
			}

			if ((controlPointStrips.Count - 4) % 3 != 0) {
				throw new ArgumentException("The number of control point strips N should be (N-4)%3==0");
			}

			var animationCurve = new AnimationCurve();

			int bezierCount = 1 + (controlPointStrips.Count - 4) / 3;
			var keyframes = new Keyframe[bezierCount + 1];

			var strip = controlPointStrips[0];
			keyframes[0] = new Keyframe(strip.x, strip.y) { weightedMode = WeightedMode.Both };

			for (int i = 0; i < bezierCount; i++) {
				int cp = i * 3;

				var strip0 = controlPointStrips[cp];
				var strip1 = controlPointStrips[cp + 1];
				var strip2 = controlPointStrips[cp + 2];
				var strip3 = controlPointStrips[cp + 3];

				float bezierLength = strip3.x - strip0.x;

				keyframes[i].outTangent = Tangent(strip0, strip1);
				keyframes[i].outWeight = Weight(strip0, strip1, bezierLength);

				keyframes[i + 1] = new Keyframe(strip3.x, strip3.y) {
					inTangent = Tangent(strip2, strip3),
					inWeight = Weight(strip2, strip3, bezierLength),
					weightedMode = WeightedMode.Both
				};
			}

			animationCurve.keys = keyframes;
			return animationCurve;
		}

		private static float Tangent(in Vector2 from, in Vector2 to) {
			var vec = to - from;
			return vec.y / vec.x;
		}

		private static float Weight(in Vector2 from, in Vector2 to, float length) {
			return (to.x - from.x) / length;
		}

		private static AnimationCurve _easeInQuad;
		private static AnimationCurve EaseInQuad => _easeInQuad ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.333333f, 0.0f),
			new Vector2(0.666667f, 0.333333f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeOutQuad;
		private static AnimationCurve EaseOutQuad => _easeOutQuad ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.333333f, 0.666667f),
			new Vector2(0.666667f, 1.0f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInOutQuad;
		private static AnimationCurve EaseInOutQuad => _easeInOutQuad ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.166667f, 0.0f),
			new Vector2(0.333333f, 0.166667f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.666667f, 0.833333f),
			new Vector2(0.833333f, 1.0f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInCubic;
		private static AnimationCurve EaseInCubic => _easeInCubic ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.333333f, 0.0f),
			new Vector2(0.666667f, 0.0f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeOutCubic;
		private static AnimationCurve EaseOutCubic => _easeOutCubic ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.333333f, 1.0f),
			new Vector2(0.666667f, 1.0f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInOutCubic;
		private static AnimationCurve EaseInOutCubic => _easeInOutCubic ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.166667f, 0.0f),
			new Vector2(0.333333f, 0.0f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.666667f, 1.0f),
			new Vector2(0.833333f, 1.0f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInQuart;
		private static AnimationCurve EaseInQuart => _easeInQuart ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.434789f, 0.006062f),
			new Vector2(0.730901f, -0.07258f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeOutQuart;
		private static AnimationCurve EaseOutQuart => _easeOutQuart ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.26909f, 1.072581f),
			new Vector2(0.565211f, 0.993938f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInOutQuart;
		private static AnimationCurve EaseInOutQuart => _easeInOutQuart ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.21739f, 0.003031f),
			new Vector2(0.365451f, -0.036291f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.634549f, 1.036290f),
			new Vector2(0.782606f, 0.996969f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInQuint;
		private static AnimationCurve EaseInQuint => _easeInQuint ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.519568f, 0.012531f),
			new Vector2(0.774037f, -0.118927f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeOutQuint;
		private static AnimationCurve EaseOutQuint => _easeOutQuint ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.225963f, 1.11926f),
			new Vector2(0.481099f, 0.987469f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInOutQuint;
		private static AnimationCurve EaseInOutQuint => _easeInOutQuint ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.259784f, 0.006266f),
			new Vector2(0.387018f, -0.059463f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.612982f, 1.059630f),
			new Vector2(0.740549f, 0.993734f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInSine;
		private static AnimationCurve EaseInSine => _easeInSine ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.360781f, -0.000436f),
			new Vector2(0.673486f, 0.486554f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeOutSine;
		private static AnimationCurve EaseOutSine => _easeOutSine ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.33093f, 0.520737f),
			new Vector2(0.641311f, 1.000333f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInOutSine;
		private static AnimationCurve EaseInOutSine => _easeInOutSine ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.180391f, -0.000217f),
			new Vector2(0.336743f, 0.243277f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.665465f, 0.760338f),
			new Vector2(0.820656f, 1.000167f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInExpo;
		private static AnimationCurve EaseInExpo => _easeInExpo ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.63696f, 0.0199012f),
			new Vector2(0.844333f, -0.0609379f),
			new Vector2(1.0f, 1.0f),
		});

		private static AnimationCurve _easeOutExpo;
		private static AnimationCurve EaseOutExpo => _easeOutExpo ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.155667f, 1.060938f),
			new Vector2(0.363037f, 0.980099f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInOutExpo;
		private static AnimationCurve EaseInOutExpo => _easeInOutExpo ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.31848f, 0.009951f),
			new Vector2(0.422167f, -0.030469f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.577833f, 1.0304689f),
			new Vector2(0.681518f, 0.9900494f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInCirc;
		private static AnimationCurve EaseInCirc => _easeInCirc ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.55403f, 0.001198f),
			new Vector2(0.998802f, 0.449801f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeOutCirc;
		private static AnimationCurve EaseOutCirc => _easeOutCirc ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.001198f, 0.553198f),
			new Vector2(0.445976f, 0.998802f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInOutCirc;
		private static AnimationCurve EaseInOutCirc => _easeInOutCirc ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.277013f, 0.000599f),
			new Vector2(0.499401f, 0.223401f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.500599f, 0.776599f),
			new Vector2(0.722987f, 0.999401f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _linear;
		private static AnimationCurve Linear => _linear ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);

		private static AnimationCurve _spring;
		private static AnimationCurve Spring => _spring ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.000000f, 0.000000f),
			new Vector2(0.080285f, 0.287602f),
			new Vector2(0.189354f, 0.568038f),
			new Vector2(0.336583f, 0.828268f),
			new Vector2(0.384005f, 0.912086f),
			new Vector2(0.450141f, 1.048536f),
			new Vector2(0.550666f, 1.079651f),
			new Vector2(0.645743f, 1.109080f),
			new Vector2(0.697447f, 0.993654f),
			new Vector2(0.779498f, 0.974607f),
			new Vector2(0.822437f, 0.964639f),
			new Vector2(0.858526f, 0.992624f),
			new Vector2(0.897999f, 1.003668f),
			new Vector2(0.931730f, 1.013104f),
			new Vector2(0.966372f, 1.006806f),
			new Vector2(1.000000f, 1.000000f)
		});

		private static AnimationCurve _easeInBounce;
		private static AnimationCurve EaseInBounce => _easeInBounce ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.030303f, 0.020833f),
			new Vector2(0.060606f, 0.020833f),
			new Vector2(0.0909f, 0.0f),

			new Vector2(0.151515f, 0.083333f),
			new Vector2(0.212121f, 0.083333f),
			new Vector2(0.2727f, 0.0f),

			new Vector2(0.393939f, 0.333333f),
			new Vector2(0.515152f, 0.333333f),
			new Vector2(0.6364f, 0.0f),

			new Vector2(0.757576f, 0.666667f),
			new Vector2(0.878788f, 1.0f),
			new Vector2(1.0f, 1.0f),
		});

		private static AnimationCurve _easeOutBounce;
		private static AnimationCurve EaseOutBounce => _easeOutBounce ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.121212f, 0.0f),
			new Vector2(0.242424f, 0.333333f),
			new Vector2(0.3636f, 1.0f),

			new Vector2(0.484848f, 0.666667f),
			new Vector2(0.606060f, 0.666667f),
			new Vector2(0.7273f, 1.0f),

			new Vector2(0.787879f, 0.916667f),
			new Vector2(0.848485f, 0.916667f),
			new Vector2(0.9091f, 1.0f),

			new Vector2(0.939394f, 0.9791667f),
			new Vector2(0.969697f, 0.9791667f),
			new Vector2(1.0f, 1.0f),
		});

		private static AnimationCurve _easeInOutBounce;
		private static AnimationCurve EaseInOutBounce => _easeInOutBounce ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.01515f, 0.010417f),
			new Vector2(0.030303f, 0.010417f),
			new Vector2(0.0455f, 0.0f),

			new Vector2(0.075758f, 0.041667f),
			new Vector2(0.106061f, 0.041667f),
			new Vector2(0.1364f, 0.0f),

			new Vector2(0.196970f, 0.166667f),
			new Vector2(0.257576f, 0.166667f),
			new Vector2(0.3182f, 0.0f),

			new Vector2(0.378788f, 0.333333f),
			new Vector2(0.439394f, 0.5f),
			new Vector2(0.5f, 0.5f),

			new Vector2(0.560606f, 0.5f),
			new Vector2(0.621212f, 0.666667f),
			new Vector2(0.6818f, 1.0f),

			new Vector2(0.742424f, 0.833333f),
			new Vector2(0.803030f, 0.833333f),
			new Vector2(0.8636f, 1.0f),

			new Vector2(0.893939f, 0.958333f),
			new Vector2(0.924242f, 0.958333f),
			new Vector2(0.9550f, 1.0f),

			new Vector2(0.969697f, 0.989583f),
			new Vector2(0.984848f, 0.989583f),
			new Vector2(1.0f, 1.0f),
		});

		private static AnimationCurve _easeInBack;
		private static AnimationCurve EaseInBack => _easeInBack ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.333333f, 0.0f),
			new Vector2(0.666667f, -0.567193f),
			new Vector2(1.0f, 1.0f),
		});

		private static AnimationCurve _easeOutBack;
		private static AnimationCurve EaseOutBack => _easeOutBack ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.333333f, 1.567193f),
			new Vector2(0.666667f, 1.0f),
			new Vector2(1.0f, 1.0f),
		});

		private static AnimationCurve _easeInOutBack;
		private static AnimationCurve EaseInOutBack => _easeInOutBack ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.166667f, 0.0f),
			new Vector2(0.333333f, -0.432485f),
			new Vector2(0.5f, 0.5f),

			new Vector2(0.666667f, 1.432485f),
			new Vector2(0.833333f, 1.0f),
			new Vector2(1.0f, 1.0f)
		});

		private static AnimationCurve _easeInElastic;
		private static AnimationCurve EaseInElastic => _easeInElastic ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.175f, 0.002507f),
			new Vector2(0.1735f, 0.0f),
			new Vector2(0.175f, 0.0f),

			new Vector2(0.4425f, -0.0184028f),
			new Vector2(0.3525f, 0.05f),
			new Vector2(0.475f, 0.0f),

			new Vector2(0.735f, -0.143095f),
			new Vector2(0.6575f, 0.383333f),
			new Vector2(0.775f, 0.0f),

			new Vector2(0.908125f, -0.586139f),
			new Vector2(0.866875f, -0.666667f),
			new Vector2(1.0f, 1.0f),
		});

		private static AnimationCurve _easeOutElastic;
		private static AnimationCurve EaseOutElastic => _easeOutElastic ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.13313f, 1.666667f),
			new Vector2(0.091875f, 1.586139f),
			new Vector2(0.225f, 1.0f),

			new Vector2(0.3425f, 0.616667f),
			new Vector2(0.265f, 1.143095f),
			new Vector2(0.525f, 1.0f),

			new Vector2(0.6475f, 0.95f),
			new Vector2(0.5575f, 1.0184028f),
			new Vector2(0.8250f, 1.0f),

			new Vector2(0.826458f, 1.0f),
			new Vector2(0.825f, 0.9974925f),
			new Vector2(1.0f, 1.0f),
		});

		private static AnimationCurve _easeInOutElastic;
		private static AnimationCurve EaseInOutElastic => _easeInOutElastic ?? BezierToAnimationCurve(new Vector2[] {
			new Vector2(0.0f, 0.0f),
			new Vector2(0.0875f, 0.001254f),
			new Vector2(0.086771f, 0.0f),
			new Vector2(0.0875f, 0.0f),

			new Vector2(0.22125f, -0.009201f),
			new Vector2(0.17625f, 0.025f),
			new Vector2(0.2375f, 0.0f),

			new Vector2(0.3675f, -0.071548f),
			new Vector2(0.32875f, 0.191667f),
			new Vector2(0.3875f, 0.0f),

			new Vector2(0.454063f, -0.293070f),
			new Vector2(0.433438f, -0.333334f),
			new Vector2(0.5f, 0.5f),

			new Vector2(0.5665625f, 1.333334f),
			new Vector2(0.5459375f, 1.293070f),
			new Vector2(0.6125f, 1.0f),

			new Vector2(0.67125f, 0.808334f),
			new Vector2(0.6325f, 1.071548f),
			new Vector2(0.7625f, 1.0f),

			new Vector2(0.82375f, 0.975f),
			new Vector2(0.77875f, 1.009201f),
			new Vector2(0.9125f, 1.0f),

			new Vector2(0.913229f, 1.0f),
			new Vector2(0.9125f, 0.9987463f),
			new Vector2(1.0f, 1.0f),
		});
		
		private static readonly AnimationCurve Constant0 = AnimationCurve.Constant(0f, 1f, 0f);
		private static readonly AnimationCurve Constant1 = AnimationCurve.Constant(0f, 1f, 1f);
	}

}
