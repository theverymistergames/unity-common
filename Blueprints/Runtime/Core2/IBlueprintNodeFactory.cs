namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintNodeFactory {


        int Count { get; }


        int AddElement();


        int AddElementCopy(IBlueprintNodeDataStorage storage, int id);


        void RemoveElement(int id);


        void Clear();


        void OptimizeDataLayout();


        IBlueprintNode CreateNode();


        IBlueprintNodeFactory CreateFactory();
    }

}
