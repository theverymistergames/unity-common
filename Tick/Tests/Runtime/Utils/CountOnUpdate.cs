﻿using MisterGames.Tick.Core;

namespace Utils {

    public class CountOnUpdate : IUpdate {
        public int Count { get; private set; }

        public void OnUpdate(float dt) {
            Count++;
        }
    }

}
