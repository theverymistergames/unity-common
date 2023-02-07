using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenInstantActionReparentTransform : ITweenInstantAction {

        public Transform target;
        public Transform newParent;
        public bool worldPositionStays = true;

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public void InvokeAction() {
            target.SetParent(newParent, worldPositionStays);
        }
    }

}
