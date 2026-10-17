using TMPro;
using UnityEngine.EventSystems;

namespace MisterGames.Dbg.Console.Core {

    public sealed class ConsoleInputField : TMP_InputField {

        // UI cancel action (Backspace, Escape) deactivates TMP_InputField and restores its original text.
        // Ignore it, so the console input is not lost while typing.
        // Do not disable the cancel action instead: re-enabling it changes the order of input callbacks
        // for keys shared with other actions, e.g. Escape is used by both UI/Cancel and UI/Pause.
        public override void OnCancel(BaseEventData eventData) { }
    }

}
