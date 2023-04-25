using MisterGames.Common.GameObjects;

namespace MisterGames.Character.Core {

    public interface ICharacterAccess {

        ITransformAdapter HeadAdapter { get; }
        ITransformAdapter BodyAdapter { get; }

        T GetPipeline<T>() where T : ICharacterPipeline;
    }

}
