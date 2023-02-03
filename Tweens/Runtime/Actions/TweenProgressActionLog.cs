using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {
    
    [Serializable]
    public sealed class TweenProgressActionLog : ITweenProgressAction {

        [SerializeField] private string _text;

        public string Text { get => _text; set => _text = value; }

        private MonoBehaviour _owner;

        public void Initialize(MonoBehaviour owner) {
            _owner = owner;
        }

        public void DeInitialize() { }

        public void OnProgressUpdate(float progress) {
            Debug.LogWarning($"Using {nameof(TweenProgressActionLog)} on game object {_owner.name}, text: {_text}, progress: {progress}");
        }
    }
    
}
