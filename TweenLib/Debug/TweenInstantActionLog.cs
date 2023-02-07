using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenInstantActionLog : ITweenInstantAction {

        public string text;

        private MonoBehaviour _owner;

        public void Initialize(MonoBehaviour owner) {
            _owner = owner;
        }

        public void DeInitialize() { }

        public void InvokeAction() {
            Debug.LogWarning($"Using {nameof(TweenInstantActionLog)} on game object {_owner.name}, text: {text}");
        }
    }

}
