namespace MisterGames.Character.Core2.View {

    public interface ICharacterViewPipeline {
        P GetProcessor<P>() where P : class;
        void SetEnabled(bool isEnabled);
    }

}
