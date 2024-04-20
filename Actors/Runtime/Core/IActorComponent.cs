namespace MisterGames.Actors {
    
    public interface IActorComponent {
        void OnAwake(IActor actor) { }
        void OnDestroy(IActor actor) { }
        
        void OnDataUpdated(IActor actor) { }
    }
    
}
