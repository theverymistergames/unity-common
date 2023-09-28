using System;

namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintFactoryStorage {


        IBlueprintFactory GetFactory(int id);


        int GetOrCreateFactory(Type factoryType);


        void RemoveFactory(int id);


        string GetFactoryPath(int id);


        void Clear();


        void OptimizeDataLayout();
    }

}
