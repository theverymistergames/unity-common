using UnityEngine;

namespace MisterGames.Character.Core2.Motion {

    [CreateAssetMenu(fileName = nameof(CharacterMassSettings), menuName = "MisterGames/Character/" + nameof(CharacterMassSettings))]
    public class CharacterMassSettings : ScriptableObject {

        [Header("Gravity")]
        [Min(0f)] public float gravityForce = -15f;

        [Header("Inertia")]
        [Min(0.001f)] public float airInertialFactor = 0.001f;
        [Min(0.001f)] public float groundInertialFactor = 20f;
    }

}
