﻿using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionMaxDistance : IInteractCondition {

        [Min(0f)] public float maxDistance;

        public bool IsMatch((IInteractiveUser, IInteractive) context, float startTime) {
            var (user, interactive) = context;
            return (user.Transform.position - interactive.Transform.position).sqrMagnitude <= maxDistance * maxDistance;
        }
    }

}
