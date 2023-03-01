namespace MisterGames.Blueprints.Meta {

    public interface IBlueprintPortLinksListener {


        void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex);
    }

}
