using System;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public class TweenProgressActions : ITweenProgressAction {

        [SerializeReference] [SubclassSelector] public ITweenProgressAction[] actions;
        
        public void Initialize(MonoBehaviour owner) {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].Initialize(owner);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].DeInitialize();
            }
        }

        public void Start() {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].Start();
            }
        }

        public void Finish() {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].Finish();
            }
        }

        public void OnProgressUpdate(float progress) {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].OnProgressUpdate(progress);
            }
        }
    }
    
}
