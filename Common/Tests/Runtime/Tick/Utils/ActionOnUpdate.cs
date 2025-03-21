﻿using System;
using MisterGames.Common.Tick;

namespace Utils {

    public class ActionOnUpdate : IUpdate {

        private readonly Action<IUpdate> _action;

        public ActionOnUpdate(Action<IUpdate> action) {
            _action = action;
        }

        public void OnUpdate(float dt) {
            _action?.Invoke(this);
        }
    }

}
