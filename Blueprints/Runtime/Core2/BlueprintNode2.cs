namespace MisterGames.Blueprints.Core2 {

    public abstract class BlueprintNode2 {
        public virtual void SetDefaultValues(IBlueprint blueprint, long id) {}

        public abstract Port[] CreatePorts(IBlueprint blueprint, long id);

        public virtual void OnInitialize(IBlueprint blueprint, long id) {}
        public virtual void OnDeInitialize(IBlueprint blueprint, long id) {}
        public virtual void OnValidate(IBlueprint blueprint, long id) {}
    }

}
