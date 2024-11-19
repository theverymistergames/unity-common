using System;
using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Logic.Damage {

    [Serializable]
    public sealed class HealthData : IActorData {
        [Min(0f)] public float health;
    }
    
}