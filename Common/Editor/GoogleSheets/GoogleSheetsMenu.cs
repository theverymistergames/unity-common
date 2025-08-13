using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.GoogleSheets {
    
    public static class GoogleSheetsMenu {

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

            for (int i = 0; i < parsers.Length; i++) {
                parsers[i].DownloadAndParse();
            }
        }
    }
    
}