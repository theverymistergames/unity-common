using System;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Data {
    
    [Serializable]
    public struct SerializedGuid : IEquatable<SerializedGuid> {
        
        [SerializeField] private int _guidLow0;
        [SerializeField] private int _guidLow1;
        [SerializeField] private int _guidHigh0;
        [SerializeField] private int _guidHigh1;
        
        public static readonly SerializedGuid Empty = new(0, 0, 0, 0);

        public SerializedGuid(int guidLow0, int guidLow1, int guidHigh0, int guidHigh1) {
            _guidLow0 = guidLow0;
            _guidLow1 = guidLow1;
            _guidHigh0 = guidHigh0;
            _guidHigh1 = guidHigh1;
        }

        public SerializedGuid(Guid guid) {
            (ulong low, ulong high) = guid.DecomposeGuid();

            NumberExtensions.UlongAsTwoInts(low, out _guidLow0, out _guidLow1);
            NumberExtensions.UlongAsTwoInts(high, out _guidHigh0, out _guidHigh1);
        }
        
        public Guid ToGuid() {
            return HashHelpers.ComposeGuid(
                NumberExtensions.TwoIntsAsUlong(_guidLow0, _guidLow1),
                NumberExtensions.TwoIntsAsUlong(_guidHigh0, _guidHigh1)
            );
        }
        
        public override int GetHashCode() {
            ulong low = NumberExtensions.TwoIntsAsUlong(_guidLow0, _guidLow1);
            ulong high = NumberExtensions.TwoIntsAsUlong(_guidHigh0, _guidHigh1);
            
            return HashHelpers.Combine(low.GetHashCode(), high.GetHashCode());
        }

        public override bool Equals(object obj) {
            return obj is SerializedGuid g && Equals(g);
        }

        public bool Equals(SerializedGuid other) {
            return _guidLow0 == other._guidLow0 &&
                   _guidLow1 == other._guidLow1 &&
                   _guidHigh0 == other._guidHigh0 && 
                   _guidHigh1 == other._guidHigh1;
        }

        public static bool operator ==(SerializedGuid lhs, SerializedGuid rhs) {
            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(SerializedGuid lhs, SerializedGuid rhs) {
            return !lhs.Equals(rhs);
        }
        
        public override string ToString() {
            ulong low = NumberExtensions.TwoIntsAsUlong(_guidLow0, _guidLow1);
            ulong high = NumberExtensions.TwoIntsAsUlong(_guidHigh0, _guidHigh1);
            
            return $"{low:X16}-{high:X16}";
        }
        
        public string ToString(string format) {
            return ToGuid().ToString(format);
        }

        public string ToString(string format, IFormatProvider provider) {
            return ToGuid().ToString(format, provider);
        }
    }
    
}