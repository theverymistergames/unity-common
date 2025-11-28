using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.GoogleSheets {
    
    public static class SheetParsersMenu {

        private static CancellationTokenSource _cts;
        
        [MenuItem("MisterGames/Sheets/Download and parse all sheets %f5")]
        private static void DownloadAllGoogleSheets() {
            if (Application.isPlaying) {
                Debug.LogWarning($"Downloading sheets is not allowed in playmode.");
                return;
            }

            var parsers = AssetDatabase
                .FindAssets($"a:assets t:{nameof(SheetParserBase)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<SheetParserBase>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();

            AsyncExt.RecreateCts(ref _cts);
            
            for (int i = 0; i < parsers.Length; i++) {
                parsers[i].DownloadAndParse(_cts.Token).Forget();
            }
        }
    }
    
}