using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Localization;

namespace MisterGames.Dialogues.Core {
    
    public interface IDialoguePrinter {
        
        UniTask PrintElement(LocalizationKey key, int roleIndex, bool instant, CancellationToken cancellationToken);
        void CancelCurrentElementPrinting(DialogueCancelMode mode);
        void ClearAllText();
    }
    
}