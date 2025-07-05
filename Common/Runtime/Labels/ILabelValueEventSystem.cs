using System;

namespace MisterGames.Common.Labels {
    
    internal interface ILabelValueEventSystem {
        
        bool Subscribe<T>(LabelValue<T> labelValue, Action<T> listener);
        bool Unsubscribe<T>(LabelValue<T> labelValue, Action<T> listener);

        bool Subscribe<T>(LabelValue<T> labelValue, ILabelValueListener<T> listener);
        bool Unsubscribe<T>(LabelValue<T> labelValue, ILabelValueListener<T> listener);
        
        void NotifyDataChanged<T>(LabelValue<T> labelValue, T data);
    }
    
}