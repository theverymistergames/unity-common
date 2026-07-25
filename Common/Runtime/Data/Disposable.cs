using System;

namespace MisterGames.Common.Data {
    
    public readonly struct Disposable<T> : IDisposable {
        
        public readonly T value;
        private readonly int _id;
        private readonly IDisposableHandler _handler;
        
        public Disposable(T value) {
            this.value = value;
            _id = 0;
            _handler = null;
        }
        
        public Disposable(T value, int id, IDisposableHandler handler) {
            this.value = value;
            _id = id;
            _handler = handler;
        }

        public void Dispose() {
            _handler?.NotifyDispose(_id);
        }
    }
    
}