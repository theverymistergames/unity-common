using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {

    [Serializable]
    public sealed class TweenInstantActionLog : ITweenInstantAction {

        [SerializeField] private string _text;

        private MonoBehaviour _owner;

        public void Initialize(MonoBehaviour owner) {
            _owner = owner;
        }

        public void DeInitialize() { }

        public void InvokeAction() {
            Debug.LogWarning($"Using {nameof(TweenInstantActionLog)} on game object {_owner.name}, text: {_text}");
        }
    }

}
