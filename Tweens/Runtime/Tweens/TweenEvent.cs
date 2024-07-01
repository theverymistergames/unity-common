using System;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Tweens {
    
    [Serializable]
    public struct TweenEvent {
        [Range(0f, 1f)] public float progress;
        [SerializeReference] [SubclassSelector] public IActorAction action;
    }
    
}