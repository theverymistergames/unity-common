using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {

    [Serializable]
    public sealed class TweenInstantActionLog : ITweenInstantAction {

        [SerializeField] private string _text;

        public string Text { get => _text; set => _text = value; }

        private MonoBehaviour _owner;

        public void Initialize(MonoBehaviour owner) {
            _owner = owner;
        }

        public void DeInitialize() { }

        public void SetText(string text) {
            _text = text;
        }

        public void InvokeAction() {
            Debug.LogWarning($"Using {nameof(TweenInstantActionLog)} on game object {_owner.name}, text: {_text}");
        }
    }

}
