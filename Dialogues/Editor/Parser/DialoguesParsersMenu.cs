using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Dialogues.Editor.Parser;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.GoogleSheets {
    
    public static class DialoguesParsersMenu {

        private static CancellationTokenSource _cts;
        
        [MenuItem("MisterGames/Sheets/Download and parse all dialogues")]
        private static void DownloadAndParseAllDialogueFiles() {
            if (Application.isPlaying) {
                Debug.LogWarning($"Downloading dialogues is not allowed in playmode.");
                return;
            }

            var parsers = AssetDatabase
                .FindAssets($"a:assets t:{nameof(DialoguesFileParser)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<DialoguesFileParser>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();

            AsyncExt.RecreateCts(ref _cts);
            
            for (int i = 0; i < parsers.Length; i++) {
                parsers[i].DownloadAndParse(_cts.Token).Forget();
            }
        }
    }
    
}