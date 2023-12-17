using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.MotionFsm {

    public sealed class CharacterMotionFsmPipeline : CharacterPipelineBase, ICharacterMotionFsmPipeline {

        [SerializeField] private MonoBehaviour _motionFsm;

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private void OnEnable() {
            _motionFsm.enabled = true;
        }

        private void OnDisable() {
            _motionFsm.enabled = false;
        }
    }

}
