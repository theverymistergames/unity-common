using MisterGames.Character.Core;
using MisterGames.Character.Processors;
using UnityEngine;

namespace MisterGames.Character {

    public interface ICharacterMotionPipeline : ICharacterPipeline {

        Vector2 MotionInput { get; }

        T GetProcessor<T>() where T : ICharacterProcessor;
    }

}
