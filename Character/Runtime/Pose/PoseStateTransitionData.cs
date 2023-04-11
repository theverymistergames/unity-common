using UnityEngine;

namespace MisterGames.Character.Pose {

    //[CreateAssetMenu(fileName = nameof(PoseStateTransitionData), menuName = "MisterGames/Character/" + nameof(PoseStateTransitionData))]
    public class PoseStateTransitionData : ScriptableObject {

        public AnimationCurve curve;
        public float duration;

    }

}
