namespace MisterGames.Actors {
    
    public interface IActorComponent {
        void OnAwake(IActor actor) { }
        void OnDestroyed(IActor actor) { }
        void OnSetData(IActor actor) { }
    }
    
}
