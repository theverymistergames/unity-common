using System;

namespace MisterGames.Common.State {
    
    public interface IEnableStateService {
        void NotifyEnable(int id, bool enable);
        void Subscribe(int id, Action<bool> callback, bool notifyOnSubscribe = true);
        void Unsubscribe(int id, Action<bool> callback);
    }
    
}