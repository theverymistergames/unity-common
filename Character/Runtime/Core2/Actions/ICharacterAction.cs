namespace MisterGames.Character.Core2.Modifiers {

    public interface ICharacterAction {
        void Apply(object source, ICharacterAccess characterAccess);
    }

}
