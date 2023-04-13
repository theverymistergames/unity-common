using UnityEngine;

namespace MisterGames.Character.Proto {

    public class CharacterFootstepsSettings : ScriptableObject {

        [Min(0.001f)] public float maxCharacterSpeed = 0.001f;
        [Min(0f)] public float stepLengthMultiplier;
        [Min(0f)] public float stepLengthMin;
        public AnimationCurve stepLengthBySpeed;

    }

}
