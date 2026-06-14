namespace MisterGames.Actors {
    
    public interface IActorTemplate {
        
        void OnApplyOnAwake(IActor actor, ActorTemplateLib lib) { }
        void OnApplyInEditor(IActor actor, ActorTemplateLib lib) { }
        
        void OnValidate(ActorTemplateLib lib) { }
    }
    
}