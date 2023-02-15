using System;
using UnityEngine;

namespace MisterGames.Common.Easing {

    [Serializable]
    public sealed class EasingCurve : IEquatable<EasingCurve> {

        public EasingType easingType = EasingType.Linear;
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public void SetCurveFromEasingType() {
            curve = easingType.ToAnimationCurve();
        }

        public bool Equals(EasingCurve other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return easingType == other.easingType && Equals(curve, other.curve);
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is EasingCurve other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine((int) easingType, curve);
        }

        public static bool operator ==(EasingCurve left, EasingCurve right) {
            return Equals(left, right);
        }

        public static bool operator !=(EasingCurve left, EasingCurve right) {
            return !Equals(left, right);
        }
    }
}
