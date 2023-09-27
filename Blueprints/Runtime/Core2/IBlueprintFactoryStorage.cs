using System;

namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintFactoryStorage {


        IBlueprintFactory GetFactory(int id);


        long AddBlueprintNodeData(Type factoryType);


        long AddBlueprintNodeDataCopy(IBlueprintFactory factory, int id);


        void RemoveBlueprintNodeData(long id);


        string GetBlueprintNodeDataPath(int factoryId, int nodeId);


        void OptimizeDataLayout();


        void Clear();
    }

}
