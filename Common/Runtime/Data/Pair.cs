using System;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public struct Pair<A, B> {

        [SerializeField] private A _first;
        [SerializeField] private B _second;

        public A First => _first;
        public B Second => _second;

        private Pair(A first, B second) {
            _first = first;
            _second = second;
        }
        
        public static Pair<A, B> Of(A first, B second) {
            return new Pair<A, B>(first, second);
        } 
        
        public override string ToString() {
            return $"[{_first}, {_second}]";
        }

    }

}