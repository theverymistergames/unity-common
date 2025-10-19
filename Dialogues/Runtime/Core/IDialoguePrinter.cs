using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Localization;

namespace MisterGames.Dialogues.Core {
    
    public interface IDialoguePrinter {
        
        UniTask PrintElement(LocalizationKey key, int roleIndex, CancellationToken cancellationToken);
        void CancelLastPrinting(bool clear = false);
        void FinishLastPrinting(float symbolDelay = -1f);
        void ClearAllText();
    }
    
}