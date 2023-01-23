using System;

namespace MisterGames.Common.Data {

    public sealed class BlackboardEvent {

        public event Action OnEmit = delegate {  };

        public void Emit() {
            OnEmit.Invoke();
        }
    }

}
