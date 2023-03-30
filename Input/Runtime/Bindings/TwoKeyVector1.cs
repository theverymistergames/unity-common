using System;
using MisterGames.Common.Maths;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    [Serializable]
    public sealed class TwoKeyVector1 : IVector1Binding {

        [SerializeField] private KeyBinding _positive;
        [SerializeField] private KeyBinding _negative;

        public float Value => _positive.IsActive().AsInt() - _negative.IsActive().AsInt();
    }

}
