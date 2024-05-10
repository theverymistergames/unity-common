namespace MisterGames.Actors {
    
    public interface IActorComponent {
        void OnAwake(IActor actor) { }
        void OnTerminate(IActor actor) { }
        void OnDataChanged(IActor actor) { }
    }
    
}
