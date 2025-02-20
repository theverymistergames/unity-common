using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Logic.Clocks;
using UnityEngine;

namespace MisterGames.ActionLib.Time {
    
    [Serializable]
    public sealed class SetTimeOfDayAction : IActorAction {

        [Range(0, 23)] public int hour;
        [Range(0, 59)] public int minute;
        [Range(0f, 59.999f)] public float second;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var now = ClockSystem.Now;
            ClockSystem.SetTime(now.Subtract(now.TimeOfDay).AddHours(hour).AddMinutes(minute).AddSeconds(second));
            return default;
        }
    }
    
}