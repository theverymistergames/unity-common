using UnityEngine;

namespace MisterGames.Character.Configs {

    //[CreateAssetMenu(fileName = nameof(MotionSettings), menuName = "MisterGames/Character/" + nameof(MotionSettings))]
    public class MotionSettings : ScriptableObject {

        [Min(0.001f)] public float inputSmoothFactor;

    }

}
