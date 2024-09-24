using System;

namespace MisterGames.Common.Data {

    [Serializable]
    public struct Vector2Parameter {

        public FloatParameter x;
        public FloatParameter y;

        public static Vector2Parameter Default() => new Vector2Parameter() {
            x = FloatParameter.Default(),
            y = FloatParameter.Default(),
        };
    }

}
