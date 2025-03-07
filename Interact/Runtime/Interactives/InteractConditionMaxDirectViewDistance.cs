﻿using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionMaxDirectViewDistance : IInteractCondition {

        [Min(0f)] public float maxDistance;

        public bool IsMatch((IInteractiveUser, IInteractive) context, float startTime) {
            var (user, interactive) = context;
            return user.IsInDirectView(interactive, out float distance) && distance <= maxDistance;
        }
    }

}
