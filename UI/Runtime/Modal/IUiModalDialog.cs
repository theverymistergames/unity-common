using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Localization;
using MisterGames.Input.Actions;

namespace MisterGames.UI.Components {
    
    public interface IUiModalDialog {
        
        IUiModalDialog SetTitle(LocalizationKey title, ILocalizationFormatter.Lambda format = null);
        IUiModalDialog SetContent(LocalizationKey content, ILocalizationFormatter.Lambda format = null);
        
        IUiModalDialog AddButton(
            LocalizationKey text,
            Action onClick,
            bool closeOnClick = true,
            bool firstSelected = true,
            ILocalizationFormatter.Lambda format = null
        );
        
        IUiModalDialog BindButtonToInputs(
            int buttonIndex,
            IReadOnlyList<InputActionRef> inputs
        );
        
        IUiModalDialog SetBackNavigation(bool canCloseOnNavigateBack, int callButton = -1);
        
        void Show();
        void Close();

        UniTask AwaitClose(CancellationToken cancellationToken = default);
    }
    
}