namespace MisterGames.Blueprints {

    public interface IBlueprintEnter {

        void OnEnterPort(IBlueprint blueprint, NodeToken token, int port);
    }

}
