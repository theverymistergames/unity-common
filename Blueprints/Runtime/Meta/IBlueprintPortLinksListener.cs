#if UNITY_EDITOR

namespace MisterGames.Blueprints.Meta {

    internal interface IBlueprintPortLinksListener {


        void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex);
    }

}

#endif
