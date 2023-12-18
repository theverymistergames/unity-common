using MisterGames.Character.Core;
using MisterGames.Character.Processors;
using MisterGames.Interact.Detectables;

namespace MisterGames.Character.View {

    public interface ICharacterViewPipeline : ICharacterPipeline {

        CameraContainer CameraContainer { get; }

        T GetProcessor<T>() where T : ICharacterProcessor;
    }

}
