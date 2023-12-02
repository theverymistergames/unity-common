using System;
using UnityEngine;

namespace MisterGames.Common.Conditions {

    [Serializable]
    public sealed class ConditionBool : ITransition {

        [SerializeField] private bool _isMatched;

        public bool IsMatched => _isMatched;

        public void Arm(ITransitionCallback callback) {
            if (_isMatched) callback.OnTransitionMatch(this);
        }

        public void Disarm() { }
    }

}
