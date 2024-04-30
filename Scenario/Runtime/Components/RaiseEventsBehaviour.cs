using System;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.Scenario.Components {
    
    public sealed class RaiseEventsBehaviour : MonoBehaviour {
        
        [SerializeField] private EventReference _eventReference;
        [SerializeField] private Trigger _trigger;
        [SerializeField] private RaiseMode _mode;
        [SerializeField] private int _value = 1;

        private enum Trigger {
            OnAwake,
            OnEnable,
            OnDisable,
            OnStart,
            OnDestroy
        }

        private enum RaiseMode {
            Add,
            SetCount
        }

        private void Awake() {
            if (_trigger == Trigger.OnAwake) Apply();
        }

        private void OnDestroy() {
            if (_trigger == Trigger.OnDestroy) Apply();
        }

        private void OnEnable() {
            if (_trigger == Trigger.OnEnable) Apply();
        }

        private void OnDisable() {
            if (_trigger == Trigger.OnDisable) Apply();
        }

        private void Start() {
            if (_trigger == Trigger.OnStart) Apply();
        }

        private void Apply() {
            switch (_mode) {
                case RaiseMode.Add:
                    _eventReference.Raise(_value);
                    break;
                case RaiseMode.SetCount:
                    _eventReference.SetCount(_value);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
}