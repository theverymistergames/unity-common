using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Editor.GoogleSheets {
    
    public abstract class SheetParserBase : ScriptableObject {

        public abstract UniTask DownloadAndParse(CancellationToken cancellationToken);
        
        protected abstract void Cancel();
        
        [Button]
        public void DownloadAndParse() {
            DownloadAndParse(default).Forget();
        }

        [Button]
        public void CancelDownload() {
            Cancel();
        }
    }
    
}