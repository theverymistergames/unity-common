using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Interactives;
using MisterGames.Common.Data;
using MisterGames.Common.Inputs;
using UnityEngine;

namespace MisterGames.ActionLib.Inputs {
    
    [Serializable]
    public sealed class GamepadVibrationWeightAction : IActorAction {

        public GamepadVibrationBehaviour gamepadVibrationBehaviour;
        public Optional<Data> left;
        public Optional<Data> right;

        [Serializable]
        public struct Data {
            [Range(0f, 1f)] public float weight;
            public Optional<float> smoothing;
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (left.HasValue) SetData(GamepadSide.Left, left.Value);
            if (right.HasValue) SetData(GamepadSide.Right, right.Value);
            
            return default;
        }

        private void SetData(GamepadSide side, Data data) {
            gamepadVibrationBehaviour.SetWeight(side, data.weight);
            if (data.smoothing.HasValue) gamepadVibrationBehaviour.SetSmoothing(side, data.smoothing.Value);
        }
    }
    
}