using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Steps {

    public delegate void StepCallback(int foot, float stepDistance, Vector3 point);

    public interface ICharacterStepsPipeline : ICharacterPipeline {
        event StepCallback OnStep;

        float StepLengthMultiplier { get; set; }
    }

}
