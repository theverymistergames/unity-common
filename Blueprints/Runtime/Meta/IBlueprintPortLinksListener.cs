namespace MisterGames.Blueprints.Meta {

    public interface IBlueprintPortLinksListener {


        void OnPortLinksChanged(BlueprintAsset blueprint, int nodeId, int portIndex);
    }

}
