using System;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.Scenario.Core {

    [CreateAssetMenu(fileName = nameof(ScenarioEvent), menuName = "MisterGames/Scenario/" + nameof(ScenarioEvent))]
    public sealed class ScenarioEvent : ScriptableObject {

        public event Action OnEmit = delegate {  };
        public bool IsEmitted { get; private set; }
        public int EmitCount { get; private set; }

        public void Emit() {
            IsEmitted = true;
            EmitCount++;
            OnEmit.Invoke();
        }

    }

}