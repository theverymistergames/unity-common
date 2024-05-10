using System;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.View {

    [Serializable]
    public struct ViewAxisClamp {
        public ClampMode mode;
        public bool absolute;
        public Vector2 bounds;
        public Vector2 springs;
        public Vector2 springFactors;
    }

}
