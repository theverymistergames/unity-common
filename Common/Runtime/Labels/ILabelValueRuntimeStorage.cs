namespace MisterGames.Common.Labels {
    
    public interface ILabelValueRuntimeStorage {

        bool TryGetData<T>(LabelLibraryRuntime<T> library, int id, out T data) where T : class;

        void SetData<T>(LabelLibraryRuntime<T> library, int id, T data) where T : class;
    }
    
}