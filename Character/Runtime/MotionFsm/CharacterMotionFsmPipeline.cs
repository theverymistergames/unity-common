using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.MotionFsm {

    public sealed class CharacterMotionFsmPipeline : CharacterPipelineBase, ICharacterMotionFsmPipeline {

        [SerializeField] private MonoBehaviour _motionFsm;

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        public override void SetEnabled(bool isEnabled) {
            _motionFsm.enabled = isEnabled;
        }
    }

}
