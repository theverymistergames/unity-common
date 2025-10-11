using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.GoogleSheets {
    
    public abstract class GoogleSheetParserBase : SheetParserBase, IGoogleSheetParser {
        
        [Header("Download Settings")]
        [SerializeField] private GoogleSheetImporter _googleSheetImporter;
        [SerializeField] private string[] _sheetIds;

        private CancellationTokenSource _cts;
        
        public override async UniTask DownloadAndParse(CancellationToken cancellationToken) {
            if (Application.isPlaying) {
                Debug.LogWarning($"Downloading Google Sheets is not allowed in playmode.");
                return;
            }
            
            AsyncExt.RecreateCts(ref _cts);
            var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token).Token;
            
            await _googleSheetImporter.DownloadAndParse(_sheetIds, this, token);
        }

        protected override void Cancel() {
            AsyncExt.DisposeCts(ref _cts);
        }

        public abstract void Parse(IReadOnlyList<SheetTable> sheetTables);

        private void Reset() {
            _googleSheetImporter = AssetDatabase
                .FindAssets($"a:assets t:{nameof(GoogleSheetImporter)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<GoogleSheetImporter>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault();
        }
    }
    
}