using UnityEngine;

namespace MisterGames.Character.Core {

    public abstract class CharacterPipelineBase : MonoBehaviour, ICharacterPipeline {
        public abstract bool IsEnabled { get; set; }
    }

}
