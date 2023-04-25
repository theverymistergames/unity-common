using UnityEngine;

namespace MisterGames.Character.Core {

    public abstract class CharacterPipelineBase : MonoBehaviour, ICharacterPipeline {

        public abstract void SetEnabled(bool isEnabled);
    }

}
