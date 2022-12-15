using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {

    [Serializable]
    public sealed class TweenInstantActionReparentTransform : ITweenInstantAction {

        [SerializeField] private Transform _target;
        [SerializeField] private Transform _newParent;
        [SerializeField] private bool _worldPositionStays = true;

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public void InvokeAction() {
            _target.SetParent(_newParent, _worldPositionStays);
        }
    }

}
