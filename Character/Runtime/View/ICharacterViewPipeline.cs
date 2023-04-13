namespace MisterGames.Character.View {

    public interface ICharacterViewPipeline {
        P GetProcessor<P>() where P : class;
        void SetEnabled(bool isEnabled);
    }

}
