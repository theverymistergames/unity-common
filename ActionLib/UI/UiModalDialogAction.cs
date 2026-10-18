using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.Input.Actions;
using MisterGames.UI.Components;
using MisterGames.UI.UiServices;
using UnityEngine;

namespace MisterGames.ActionLib.UI {
    
    [Serializable]
    public sealed class UiModalDialogAction : IActorAction {
        
        public Canvas canvas;
        public UiModalDialog modalDialogPrefab;

        [Header("Content")]
        public LocalizationKey title;
        public LocalizationKey content;
        
        [Header("Content Format")]
        [SerializeReference] [SubclassSelector] public ILocalizationFormatter titleFormatter;
        [SerializeReference] [SubclassSelector] public ILocalizationFormatter contentFormatter;
        
        [Header("Navigation")]
        public bool canCloseOnNavigateBack;
        [Min(0)] public int navigateBackButtonIndex;
        public ButtonData[] buttons;

        [Header("Actions")]
        [SerializeReference] [SubclassSelector] public IActorAction afterShow;

        [Serializable]
        public struct ButtonData {
            public LocalizationKey text;
            [SerializeReference] [SubclassSelector] public ILocalizationFormatter textFormatter;
            public bool closeOnClick;
            public bool firstSelected;
            public InputActionRef[] bindInputs;
        }
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var dialog = Services.Get<IUiModalDialogService>().CreateModalDialog(modalDialogPrefab, canvas);

            dialog.SetTitle(title, (LocalizationKey key, Locale locale, ref string value) => titleFormatter?.Format(key, locale, ref value));
            dialog.SetContent(content, (LocalizationKey key, Locale locale, ref string value) => contentFormatter?.Format(key, locale, ref value));

            dialog.SetBackNavigation(canCloseOnNavigateBack, navigateBackButtonIndex);

            for (int i = 0; i < buttons.Length; i++) {
                var buttonData = buttons[i];

                dialog.AddButton(buttonData.text, onClick: null, buttonData.closeOnClick, buttonData.firstSelected, 
                    format: (LocalizationKey key, Locale locale, ref string value) => buttonData.textFormatter?.Format(key, locale, ref value));

                dialog.BindButtonToInputs(i, buttonData.bindInputs);
            }
            
            dialog.Show();
            
            if (afterShow != null) {
                await afterShow.Apply(context, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            await dialog.AwaitClose(cancellationToken);
        }
    }
    
}