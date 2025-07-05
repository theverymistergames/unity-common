namespace MisterGames.Common.Labels {
    
    public interface ILabelValueListener<T> {
    
        void OnDataChanged(LabelValue<T> labelValue, T data);
    }
    
}