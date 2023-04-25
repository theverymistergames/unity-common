using MisterGames.Character.Core;
using MisterGames.Character.Processors;

namespace MisterGames.Character.View {

    public interface ICharacterViewPipeline : ICharacterPipeline {
        T GetProcessor<T>() where T : ICharacterProcessor;
    }

}
