using MisterGames.Blueprints.Core;

namespace MisterGames.Blueprints {

    internal static class BlueprintUtils {
        
        internal static IBlueprint AsIBlueprint(this Blueprint blueprint) => blueprint;
        
        internal static IBlueprintNode AsIBlueprintNode(this Blueprint blueprint) => blueprint;
        
        internal static IBlueprintNode AsIBlueprintNode(this BlueprintNode node) => node;
        
    }

}