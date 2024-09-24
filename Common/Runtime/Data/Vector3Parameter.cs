using System;

namespace MisterGames.Common.Data {

    [Serializable]
    public struct Vector3Parameter {

        public FloatParameter x;
        public FloatParameter y;
        public FloatParameter z;

        public static readonly Vector3Parameter Default = new Vector3Parameter() {
            x = FloatParameter.Default,
            y = FloatParameter.Default,
            z = FloatParameter.Default,
        };
    }

}
