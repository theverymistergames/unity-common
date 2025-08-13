using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MisterGames.Common.Editor.GoogleSheets {
    
    public sealed class SheetTable {

        public string Title { get; }
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }

        private readonly List<string> _rows = new();
        private readonly List<string> _columns = new();
        private readonly List<string> _data = new();

        public SheetTable(string title) {
            Title = title;
        }
        
        public string GetRow(int row) {
            return row < RowCount ? _rows[row] : null;
        }
        
        public string GetColumn(int column) {
            return column < ColumnCount ? _columns[column] : null;
        }

        public string GetData(int row, int column) {
            return row < RowCount && column < ColumnCount ? _data[row * ColumnCount + column] : null;
        }

        public void SetRow(int row, string key) {
            RowCount = Mathf.Max(row + 1, RowCount);
            EnsureCapacity(_rows, row + 1);
            
            _rows[row] = key;
        }

        public void SetColumn(int column, string key) {
            ColumnCount = Mathf.Max(column + 1, ColumnCount);
            EnsureCapacity(_columns, column + 1);
            
            _columns[column] = key;
        }

        public void SetData(int row, int column, string data) {
            RowCount = Mathf.Max(row + 1, RowCount);
            ColumnCount = Mathf.Max(column + 1, ColumnCount);
            
            EnsureCapacity(_data, RowCount * ColumnCount);
            
            _data[row * ColumnCount + column] = data;
        }

        private static void EnsureCapacity<T>(IList<T> list, int size) {
            if (list.Count >= size) return;

            int addSize = size - list.Count;

            for (int i = 0; i < addSize; i++) {
                list.Add(default);
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();
            
            sb.AppendLine("Columns: ");
            for (int i = 0; i < ColumnCount; i++) {
                sb.AppendLine($"[{i}] [{_columns[i]}]");
            }
            
            sb.AppendLine("Rows: ");
            for (int i = 0; i < RowCount; i++) {
                sb.AppendLine($"[{i}] [{_rows[i]}]");
            }

            sb.AppendLine("Data: ");
            for (int i = 0; i < ColumnCount * RowCount; i++) {
                int r = i / ColumnCount;
                int c = i % ColumnCount;
                sb.AppendLine($"[c #{c} {_columns[c]}, r #{r} {_rows[r]}] [{_data[i]}]");
            }
            
            return sb.ToString();
        }
    }
    
}