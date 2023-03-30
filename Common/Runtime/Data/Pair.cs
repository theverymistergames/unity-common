using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public struct Pair<Ta, Tb> : IEquatable<Pair<Ta, Tb>> {

        [SerializeField] private Ta _a;
        [SerializeField] private Tb _b;

        public Ta A => _a;
        public Tb B => _b;

        public static Pair<Ta, Tb> Of(Ta a = default, Tb b = default) => new Pair<Ta, Tb>(a, b);

        public Pair(Ta a = default, Tb b = default) {
            _a = a;
            _b = b;
        }

        public void Deconstruct(out Ta a, out Tb b) {
            a = _a;
            b = _b;
        }

        public bool Equals(Pair<Ta, Tb> other) {
            return EqualityComparer<Ta>.Default.Equals(_a, other._a) &&
                   EqualityComparer<Tb>.Default.Equals(_b, other._b);
        }

        public override bool Equals(object obj) {
            return obj is Pair<Ta, Tb> other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_a, _b);
        }

        public static bool operator ==(Pair<Ta, Tb> left, Pair<Ta, Tb> right) {
            return left.Equals(right);
        }

        public static bool operator !=(Pair<Ta, Tb> left, Pair<Ta, Tb> right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"Pair(a = {_a}, b = {_b})";
        }
    }

}
