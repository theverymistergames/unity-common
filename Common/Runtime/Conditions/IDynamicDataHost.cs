using System;
using System.Collections.Generic;

namespace MisterGames.Common.Conditions {
    
    public interface IDynamicDataHost {
        void OnSetDataTypes(HashSet<Type> types);

        void OnSetData(IDynamicDataProvider provider);
    }

}
