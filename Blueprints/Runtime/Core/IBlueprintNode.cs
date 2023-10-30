namespace MisterGames.Blueprints {

    public interface IBlueprintNode {

        void CreatePorts(IBlueprintMeta meta, NodeId id);

        void OnSetDefaults(IBlueprintMeta meta, NodeId id) {}
        void OnValidate(IBlueprintMeta meta, NodeId id) {}

        void OnInitialize(IBlueprint blueprint, NodeId id) {}
        void OnDeInitialize(IBlueprint blueprint, NodeId id) {}
    }

}
