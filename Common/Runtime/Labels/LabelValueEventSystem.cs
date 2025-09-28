using System;
using System.Runtime.CompilerServices;
using MisterGames.Common.Data;

namespace MisterGames.Common.Labels {
    
    internal sealed class LabelValueEventSystem : ILabelValueEventSystem, IDisposable {
        
        private readonly TreeMap<LabelValue, object> _listenerTree = new();

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
            if (!_listenerTree.TryGetNode(ConvertToPlainLabelValue(labelValue), out int root)) return;
            
            int i = _listenerTree.GetChild(root);
            while (i >= 0) {
                int next = _listenerTree.GetNext(i);

                switch (_listenerTree.GetValueAt(i)) {
                    case Action<T> actionListener:
                        actionListener.Invoke(data);
                        break;
                    
                    case ILabelValueListener<T> interfaceListener:
                        interfaceListener.OnDataChanged(labelValue, data);
                        break;
                }

                i = next;
            }
        }

        private bool SubscribeListener<T>(LabelValue<T> labelValue, object listener) {
            if (labelValue.library == null) return false;
            int root = _listenerTree.GetOrAddNode(ConvertToPlainLabelValue(labelValue));
            
            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (AreEqualListeners(_listenerTree.GetValueAt(i), listener)) return false;
            }

            _listenerTree.AddEndPoint(root, listener);
            return true;
        }

        private bool UnsubscribeListener<T>(LabelValue<T> labelValue, object listener) {
            if (labelValue.library == null ||
                !_listenerTree.TryGetNode(ConvertToPlainLabelValue(labelValue), out int root)) 
            {
                return false;
            }
            
            for (int i = _listenerTree.GetChild(root); i >= 0; i = _listenerTree.GetNext(i)) {
                if (!AreEqualListeners(_listenerTree.GetValueAt(i), listener)) continue;

                _listenerTree.RemoveNodeAt(i);
                return true;
            }

            return false;
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