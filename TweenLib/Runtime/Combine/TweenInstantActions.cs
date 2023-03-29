﻿using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public class TweenInstantActions : ITweenInstantAction {

        [SerializeReference] [SubclassSelector] public List<ITweenInstantAction> actions;

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

        public void InvokeAction() {
            for (int i = 0; i < actions.Count; i++) {
                actions[i].InvokeAction();
            }
        }
    }

}
