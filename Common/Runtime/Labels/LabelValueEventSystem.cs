using System;
using System.Runtime.CompilerServices;
using MisterGames.Common.Data;

namespace MisterGames.Common.Labels {
    
    internal sealed class LabelValueEventSystem : ILabelValueEventSystem, IDisposable {
        
        private readonly MultiValueDictionary<LabelValue, object> _listenerTree = new();

        public void Dispose() {
            _listenerTree.Clear();
        }

        public bool Subscribe<T>(LabelValue<T> labelValue, Action<T> listener) {
            return SubscribeListener(labelValue, listener);
        }

        public bool Unsubscribe<T>(LabelValue<T> labelValue, Action<T> listener) {
            return UnsubscribeListener(labelValue, listener);
        }

        public bool Subscribe<T>(LabelValue<T> labelValue, ILabelValueListener<T> listener) {
            return SubscribeListener(labelValue, listener);
        }

        public bool Unsubscribe<T>(LabelValue<T> labelValue, ILabelValueListener<T> listener) {
            return UnsubscribeListener(labelValue, listener);
        }

        public void NotifyDataChanged<T>(LabelValue<T> labelValue, T data) {
            var key = ConvertToPlainLabelValue(labelValue);
            int count = _listenerTree.GetCount(key);

            for (int i = 0; i < count; i++) {
                switch (_listenerTree.GetValueAt(key, i)) {
                    case Action<T> actionListener:
                        actionListener.Invoke(data);
                        break;
                    
                    case ILabelValueListener<T> interfaceListener:
                        interfaceListener.OnDataChanged(labelValue, data);
                        break;
                }
            }
        }

        private bool SubscribeListener<T>(LabelValue<T> labelValue, object listener) {
            var key = ConvertToPlainLabelValue(labelValue);
            int count = _listenerTree.GetCount(key);

            for (int i = 0; i < count; i++) {
                if (AreEqualListeners(_listenerTree.GetValueAt(key, i), listener)) return false;
            }

            _listenerTree.AddValue(key, listener);
            return true;
        }

        private bool UnsubscribeListener<T>(LabelValue<T> labelValue, object listener) {
            return _listenerTree.RemoveValue(ConvertToPlainLabelValue(labelValue), listener);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static LabelValue ConvertToPlainLabelValue<T>(LabelValue<T> labelValueT) {
            return new LabelValue(labelValueT.library, labelValueT.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AreEqualListeners(object l0, object l1) {
            return l0 is not null && l0.Equals(l1);
        }
    }
    
}