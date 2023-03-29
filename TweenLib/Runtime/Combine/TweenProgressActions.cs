using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public class TweenProgressActions : ITweenProgressAction {

        [SerializeReference] [SubclassSelector] public List<ITweenProgressAction> actions;
        
        public void Initialize(MonoBehaviour owner) {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].Initialize(owner);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].DeInitialize();
            }
        }

        public void Start() {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].Start();
            }
        }

        public void Finish() {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].Finish();
            }
        }

        public void OnProgressUpdate(float progress) {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].OnProgressUpdate(progress);
            }
        }
    }
    
}
