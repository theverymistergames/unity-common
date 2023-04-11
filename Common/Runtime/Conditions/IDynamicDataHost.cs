using System;

namespace MisterGames.Common.Conditions {
    
    public interface IDynamicDataHost {
        Type DataType { get; }

        void OnSetData(IDynamicDataProvider provider);
    }

}
