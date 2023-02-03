namespace MisterGames.Blueprints.Runtime.Core {

    internal interface IBlueprintConnectionListener {
        void OnAddLinkToPort(BlueprintAsset blueprint, int nodeId, int portIndex);
        void OnRemoveLinkToPort(BlueprintAsset blueprint, int nodeId, int portIndex);

        void OnAddLinkFromPort(BlueprintAsset blueprint, int nodeId, int portIndex);
        void OnRemoveLinkFromPort(BlueprintAsset blueprint, int nodeId, int portIndex);
    }

}
