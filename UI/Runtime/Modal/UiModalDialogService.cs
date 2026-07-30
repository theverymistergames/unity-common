using System;
using MisterGames.Common.Pooling;
using MisterGames.UI.Components;
using UnityEngine;

namespace MisterGames.UI.UiServices {
    
    public sealed class UiModalDialogService : IUiModalDialogService, IDisposable {

        private UiModalDialog _defaultModalDialogPrefab;
        
        public void Initialize(UiModalDialog defaultModalDialogPrefab) {
            _defaultModalDialogPrefab = defaultModalDialogPrefab;
        }

        public void Dispose() {
            
        }

        public IUiModalDialog CreateModalDialog(UiModalDialog prefab, Canvas canvas) {
            var dialog = PrefabPool.Main.Get(prefab, canvas.transform, active: false);
            dialog.SetTitle(default);
            dialog.SetContent(default);
            dialog.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            dialog.transform.localScale = Vector3.one;
            return dialog;
        }
        
        public IUiModalDialog CreateModalDialogDefault(Canvas canvas) {
            return CreateModalDialog(_defaultModalDialogPrefab, canvas);
        }
    }
    
}