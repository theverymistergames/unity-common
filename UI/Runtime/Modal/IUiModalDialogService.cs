using MisterGames.UI.Components;
using UnityEngine;

namespace MisterGames.UI.UiServices {
    
    public interface IUiModalDialogService {
        IUiModalDialog CreateModalDialog(UiModalDialog prefab, Canvas canvas);
        IUiModalDialog CreateModalDialogDefault(Canvas canvas);
    }
    
}