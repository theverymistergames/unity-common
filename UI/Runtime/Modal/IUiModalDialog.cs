using System;
using MisterGames.Common.Localization;

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
        
        IUiModalDialog SetBackNavigation(bool canCloseOnNavigateBack, int callButton = -1);
        
        void Show();
        void Close();
    }
    
}