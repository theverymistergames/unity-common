using UnityEngine;

namespace MisterGames.Character.Configs {

    //[CreateAssetMenu(fileName = nameof(MassSettings), menuName = "MisterGames/Character/" + nameof(MassSettings))]
    public class MassSettings : ScriptableObject {

        [Header("Gravity")]
        [Min(0f)] public float gravityForce = 9.8f;

        [Header("Inertia")]
        [Min(0.001f)] public float airInertialFactor = 1f;
        [Min(0.001f)] public float groundInertialFactor = 1f;

    }

}
