using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Steps {

    public delegate void StepCallback(int foot, float distance, Vector3 point);

    public interface ICharacterStepsPipeline : ICharacterPipeline {
        event StepCallback OnStep;
    }

}
