using UnityEngine;

namespace MisterGames.Common.Maths {

    public static class QuaternionUtils {

        public static Quaternion Inverted(this Quaternion quaternion) {
            return Quaternion.Inverse(quaternion);
        }

    }

}