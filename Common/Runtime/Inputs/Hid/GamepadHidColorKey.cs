using System;
using UnityEngine;

namespace MisterGames.Common.Inputs {

    [Serializable]
    public struct GamepadHidColorKey {

        [Range(0f, 1f)] public float progress;
        public GamepadHidColorState state;

        public GamepadHidColorKey(float progress, GamepadHidColorState state) {
            this.progress = progress;
            this.state = state;
        }
    }

}
