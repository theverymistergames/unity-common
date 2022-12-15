using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {

    [Serializable]
    public class TweenInstantActionLog : ITweenInstantAction {

        [SerializeField] private string _text;

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public void InvokeAction() {
            Debug.Log(_text);
        }
    }

}
