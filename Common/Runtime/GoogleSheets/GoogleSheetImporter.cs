using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace MisterGames.Common.GoogleSheets {
    
    public sealed class GoogleSheetImporter {
        
        public readonly struct Result<T> {
            
            public readonly ResultType resultType;
            public readonly T data;
            public readonly string message;
            
            public Result(ResultType resultType, T data, string message) {
                this.resultType = resultType;
                this.data = data;
                this.message = message;
            }

            public static Result<T> Success(T data) => new(ResultType.Success, data, message: null);
            public static Result<T> Error(string message) => new(ResultType.Error, data: default, message);
        }
        
        public enum ResultType {
            Success,
            Error,
        }
        
        private readonly SheetsService _service;
        
        public GoogleSheetImporter(string credentialsJson) {
            _service = new SheetsService(new BaseClientService.Initializer {
                HttpClientInitializer = GoogleCredential.FromJson(credentialsJson).CreateScoped(SheetsService.Scope.Spreadsheets)
            });
        }
        
        public async UniTask<Result<IReadOnlyList<string>>> GetTitles(string sheetId) {
            try {
                var request = _service.Spreadsheets.Get(sheetId);
                var spreadsheet = await request.ExecuteAsync();
                string[] data = spreadsheet == null 
                    ? Array.Empty<string>() 
                    : spreadsheet.Sheets.Select(x => x.Properties.Title).ToArray();
                
                return Result<IReadOnlyList<string>>.Success(data);
            }
            catch (Exception e) {
                return Result<IReadOnlyList<string>>.Error(e.Message);
            }
        }
        
        public async UniTask<Result<SheetTable>> DownloadSheet(string sheetId, string sheetName) {
            try {
                string range = $"{sheetName}!A1:Z";
                var request = _service.Spreadsheets.Values.Get(sheetId, range);
                var response = await request.ExecuteAsync();
                
                var table = new SheetTable();
                var rows = response.Values;

                int startRow = -1;
                int startColumn = -1;
                
                for (int r = 0; r < rows.Count; r++) {
                    var row = rows[r];
                    
                    for (int c = 0; c < row.Count; c++) {
                        if (startColumn >= 0 && c < startColumn) continue;
                        
                        string cell = row[c]?.ToString();

                        if (startRow < 0) {
                            if (string.IsNullOrWhiteSpace(cell)) continue;

                            startRow = r;
                            startColumn = c;
                            continue;
                        }
                        
                        if (string.IsNullOrWhiteSpace(cell)) continue;
                        
                        if (r == startRow) {
                            table.SetColumn(c - startColumn - 1, cell);
                            continue;
                        }
                        
                        if (c == startColumn) {
                            table.SetRow(r - startRow - 1, cell);
                            continue;
                        }
                        
                        table.SetData(r - startRow - 1, c - startColumn - 1, cell);
                    }
                }
                
                return Result<SheetTable>.Success(table);
            }
            catch (Exception e) {
                return Result<SheetTable>.Error(e.Message);
            }
        }
    }
    
}