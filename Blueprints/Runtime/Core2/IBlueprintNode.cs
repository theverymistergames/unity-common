namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintNode {

        void CreatePorts(IBlueprintMeta meta, long id);

        void SetDefaultValues(long id) {}
        void OnValidate(IBlueprintMeta meta, long id) {}

        void OnInitialize(IBlueprint blueprint, long id) {}
        void OnDeInitialize(IBlueprint blueprint, long id) {}
    }

}
