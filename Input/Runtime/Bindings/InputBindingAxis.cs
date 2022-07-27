using System;
using MisterGames.Common.Maths;
using MisterGames.Input.Core;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    public interface IInputBindingAxis : IInputBinding {
        float GetValue();
    }

    [Serializable]
    public struct InputBindingAxisKey : IInputBindingAxis {

        [SerializeField] private KeyBinding _positive;
        [SerializeField] private KeyBinding _negative;
        
        public void Init() { }

        public void Terminate() { }

        public float GetValue() {
            return _positive.IsActive().ToInt() - _negative.IsActive().ToInt();
        }
    }

}
