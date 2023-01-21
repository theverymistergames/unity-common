using MisterGames.Common.Data;

namespace MisterGames.Blueprints {

    public interface IBlueprintHost {
        BlueprintRunner Runner { get; }
        RuntimeBlackboard Blackboard { get; }
    }

}
