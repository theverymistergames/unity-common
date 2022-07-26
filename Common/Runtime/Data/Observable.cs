using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public abstract class ObservableBase {
        public abstract void NotifyIfChanged();
    }
    
    [Serializable]
    public class Observable<T> : ObservableBase {
        
        public delegate void ValueChanged(T newValue);
        public delegate void ValueChangedWithOld(T oldValue, T newValue);

        public event ValueChanged OnValueChanged = delegate {  };
        public event ValueChangedWithOld OnValueChangedWithOld = delegate {  };

        public T Value {
            get => _value;
            set {
                _prevValue = _value;
                _value = value;
                NotifyIfChanged();
            }
        }
        
        [SerializeField]
        private T _value;

        private T _prevValue;

        private EqualityComparer<T> _comparer = EqualityComparer<T>.Default;
        
        private Observable(T value) {
            _value = value;
        }

        public override void NotifyIfChanged() {
            if (_comparer == null) return;
            if (_comparer.Equals(_prevValue, _value)) return;
            
            var oldValue = _prevValue;
            _prevValue= _value;
            
            OnValueChanged.Invoke(_value);
            OnValueChangedWithOld.Invoke(oldValue, _value);
        }

        public static Observable<T> From(T value) {
            return new Observable<T>(value);
        }

    }

}