using UnityEngine;

namespace MisterGames.Character.Configs {

    [CreateAssetMenu(fileName = nameof(CharacterViewSettings), menuName = "MisterGames/Character/" + nameof(CharacterViewSettings))]
    public class CharacterViewSettings : ScriptableObject {

        [Min(0.001f)] public float sensitivityHorizontal = 0.15f;
        [Min(0.001f)] public float sensitivityVertical = 0.15f;
        [Min(0.001f)] public float viewSmoothFactor = 20f;
    }

}
