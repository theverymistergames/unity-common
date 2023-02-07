using MisterGames.Blueprints.Meta;

namespace MisterGames.Blueprints.Runtime.Core {

    internal interface IBlueprintPortLinksListener {
        void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex);
    }

}
