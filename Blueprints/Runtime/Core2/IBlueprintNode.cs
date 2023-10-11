namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintNode {

        Port[] CreatePorts(IBlueprintMeta blueprintMeta, long id);

        void SetDefaultValues(IBlueprintMeta blueprintMeta, long id) {}
        void OnValidate(IBlueprintMeta blueprintMeta, long id) {}

        void OnInitialize(IBlueprint blueprint, long id) {}
        void OnDeInitialize(IBlueprint blueprint, long id) {}
    }

}
