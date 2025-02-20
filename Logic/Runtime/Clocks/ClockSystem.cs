using System;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Clocks {
    
    public sealed class ClockSystem : MonoBehaviour, IUpdate {
        
        public static DateTime Now { get; private set; } = new();

        private double _lastTime;
        
        public static void SetTime(DateTime dateTime) {
            Now = dateTime;
        }

        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
            _lastTime = Time.unscaledTimeAsDouble;
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            double time = Time.unscaledTimeAsDouble;
            double delta = time - _lastTime;
            _lastTime = time;
            
            Now = Now.AddSeconds(delta * Time.timeScale);
        }
    }
    
}