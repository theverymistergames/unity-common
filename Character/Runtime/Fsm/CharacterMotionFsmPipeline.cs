using UnityEngine;

namespace MisterGames.Character.Fsm {

    public sealed class CharacterMotionFsmPipeline : MonoBehaviour, ICharacterMotionFsmPipeline {

        [SerializeField] private MonoBehaviour _motionFsm;

        public void SetEnabled(bool isEnabled) {
            _motionFsm.enabled = isEnabled;
        }
    }

}
