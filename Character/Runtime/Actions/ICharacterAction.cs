using MisterGames.Character.Core;

namespace MisterGames.Character.Actions {

    public interface ICharacterAction {
        void Apply(object source, ICharacterAccess characterAccess);
    }

}
