using MisterGames.Common.Labels.Base;

namespace MisterGames.Common.Labels {
    
    public interface ILabelValueRuntimeStorage {

        bool TryGetData<T>(LabelLibraryBase<T> library, int id, out T data) where T : class;

        void SetData<T>(LabelLibraryBase<T> library, int id, T data) where T : class;
        
        bool RemoveData<T>(LabelLibraryBase<T> library, int id) where T : class;
    }
    
}