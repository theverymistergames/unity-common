using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MisterGames.Common.Inputs {

    [Serializable]
    public struct GamepadHidColorState : IEquatable<GamepadHidColorState> {

        public Color color;
        [Range(0f, 1f)] public float intensity;
        [FormerlySerializedAs("lightPosition")] public LightOptions lightOptions;

        public GamepadHidColorState(Color color, float intensity = 1f, LightOptions lightOptions = LightOptions.None) {
            this.color = color;
            this.intensity = intensity;
            this.lightOptions = lightOptions;
        }

        public bool Equals(GamepadHidColorState other) {
            return color.Equals(other.color) &&
                   intensity.Equals(other.intensity) &&
                   lightOptions == other.lightOptions;
        }

        public override bool Equals(object obj) => obj is GamepadHidColorState other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(color, intensity, (int) lightOptions);
        public static bool operator ==(GamepadHidColorState a, GamepadHidColorState b) => a.Equals(b);
        public static bool operator !=(GamepadHidColorState a, GamepadHidColorState b) => !a.Equals(b);
    }

}
