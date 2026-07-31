using System;

namespace MisterGames.UI.Data {
    
    public interface IUiElementAnimator {
        
        event Action<UiElementState> OnStateChanged;
        UiElementState CurrentState { get; }
        
        void AnimateState(UiElementState state);
        void SetForceMinState(UiElementState state);
        void ResetForceMinState();
        void SetBlockedState(bool blocked);
    }
    
}