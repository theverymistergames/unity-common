﻿using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionMaxUsers : IInteractCondition {

        [Min(0)] public int maxUsers;

        public bool IsMatch((IInteractiveUser, IInteractive) context, float startTime) {
            var (user, interactive) = context;
            return interactive.Users.Count <= maxUsers;
        }
    }

}
