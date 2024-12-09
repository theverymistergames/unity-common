using System;

namespace MisterGames.Common.Labels {
    
    public abstract class LabelLibraryBaseT<T> : LabelLibraryBase {

        public override Type GetDataType() {
            return typeof(T);
        }
        
        public abstract bool TryGetData(int array, int value, out T data);
    }
    
}