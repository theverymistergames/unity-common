using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Easing;
using MisterGames.Common.Inputs;
using MisterGames.Common.Inputs.DualSense;
using MisterGames.Common.Labels;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Inputs {
    
    [Serializable]
    public sealed class SetGamepadTriggerEffectAction : IActorAction {
        
        public GamepadSide side;
        public TriggerEffect effect;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            DeviceService.Instance.DualSenseAdapter.SetTriggerEffect(side, effect);
            return default;
        }
        
    }
    
}