namespace MisterGames.Blueprints {

    public interface IBlueprintNode {

        void CreatePorts(IBlueprintMeta meta, NodeId id);

        void OnSetDefaults(IBlueprintMeta meta, NodeId id) {}
        void OnValidate(IBlueprintMeta meta, NodeId id) {}

        void OnInitialize(IBlueprint blueprint, NodeToken token) {}
        void OnDeInitialize(IBlueprint blueprint, NodeToken token) {}
    }

}
