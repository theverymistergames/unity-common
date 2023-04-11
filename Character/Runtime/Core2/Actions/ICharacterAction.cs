using MisterGames.Character.Core2.Access;

namespace MisterGames.Character.Core2.Actions {

    public interface ICharacterAction {
        void Apply(object source, ICharacterAccess characterAccess);
    }

}
