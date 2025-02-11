﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetViewSmoothing : IActorAction {

        [Min(0)] public float viewSmoothFactor = 20f;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context.GetComponent<CharacterViewPipeline>().ApplySmoothing(viewSmoothFactor);
            return default;
        }
    }
    
}
