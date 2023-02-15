using System;
using UnityEngine;

namespace MisterGames.Common.Easing {

    [Serializable]
    public sealed class EasingCurve : IEquatable<EasingCurve> {

        [SerializeField] private EasingType _easingType = EasingType.Linear;

        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public EasingType easingType {
            get => _easingType;
            set {
                _easingType = value;
                curve = _easingType.ToAnimationCurve();
            }
        }

        public bool Equals(EasingCurve other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _easingType == other._easingType && Equals(curve, other.curve);
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is EasingCurve other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine((int) _easingType, curve);
        }

        public static bool operator ==(EasingCurve left, EasingCurve right) {
            return Equals(left, right);
        }

        public static bool operator !=(EasingCurve left, EasingCurve right) {
            return !Equals(left, right);
        }
    }
}
