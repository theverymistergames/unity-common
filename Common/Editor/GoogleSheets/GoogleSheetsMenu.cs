using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.GoogleSheets {
    
    public static class GoogleSheetsMenu {

        private static CancellationTokenSource _cts;
        
        [MenuItem("MisterGames/Google/Download all Google Sheets")]
        private static void DownloadAllGoogleSheets() {
            if (Application.isPlaying) {
                Debug.LogWarning($"Downloading Google Sheets is not allowed in playmode.");
                return;
            }

            var parsers = AssetDatabase
                .FindAssets($"a:assets t:{nameof(GoogleSheetParserBase)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<GoogleSheetParserBase>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();

            AsyncExt.RecreateCts(ref _cts);
            
            for (int i = 0; i < parsers.Length; i++) {
                parsers[i].DownloadAndParse(_cts.Token).Forget();
            }
        }
    }
    
}