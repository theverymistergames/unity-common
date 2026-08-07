using System;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Clocks {
    
    public sealed class ClockSystem : MonoBehaviour, IUpdate {
        
        public static DateTime Now { get; private set; } = new();
        
        public static void SetTime(DateTime dateTime) {
            Now = dateTime;
        }

        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            Now = Now.AddSeconds(Time.deltaTime);
        }
    }
    
}