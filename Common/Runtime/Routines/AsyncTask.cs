using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Routines {

    public interface IAsyncTaskReadOnly {
        bool IsDone { get; }
        float Progress { get; }
    }

    public sealed class AsyncTask : IAsyncTaskReadOnly {

        public static IAsyncTaskReadOnly Done = new AsyncTaskDone();
        
        public bool IsDone => IsAllDone();
        public float Progress => GetTotalProgress();
        
        private readonly List<AsyncOperation> _operations = new List<AsyncOperation>();
        private readonly List<IAsyncTaskReadOnly> _tasks = new List<IAsyncTaskReadOnly>();

        private struct AsyncTaskDone : IAsyncTaskReadOnly {
            public bool IsDone => true;
            public float Progress => 1f;
        }
        
        public void Add(AsyncOperation operation) {
            if (operation.isDone) return;
            _operations.Add(operation);
        }

        public void Add(IAsyncTaskReadOnly task) {
            if (task.IsDone) return;
            _tasks.Add(task);
        }

        private bool IsAllDone() {
            bool allOperationsDone = true;
            bool allTasksDone = true;
            
            int operationsCount = _operations.Count;
            for (int i = 0; i < operationsCount; i++) {
                var operation = _operations[i];
                
                if (operation == null || operation.isDone) {
                    _operations.RemoveAt(i--);
                    operationsCount--;
                    continue;
                }

                allOperationsDone = false;
            }
            
            int tasksCount = _tasks.Count;
            for (int i = 0; i < tasksCount; i++) {
                var task = _tasks[i];
                
                if (task == null || task.IsDone) {
                    _tasks.RemoveAt(i--);
                    tasksCount--;
                    continue;
                }

                allTasksDone = false;
            }

            return allOperationsDone && allTasksDone;
        }
        
        private float GetTotalProgress() {
            int inProgressCount = 0;
            float progress = 0f;

            int operationsCount = _operations.Count;
            for (int i = 0; i < operationsCount; i++) {
                var operation = _operations[i];
                
                if (operation == null || operation.isDone) {
                    _operations.RemoveAt(i--);
                    operationsCount--;
                    continue;
                }
                
                inProgressCount++;
                progress += operation.progress;
            }
            
            int tasksCount = _tasks.Count;
            for (int i = 0; i < tasksCount; i++) {
                var task = _tasks[i];
                
                if (task == null || task.IsDone) {
                    _tasks.RemoveAt(i--);
                    tasksCount--;
                    continue;
                }
                
                inProgressCount++;
                progress += task.Progress;
            }

            return inProgressCount == 0 ? 1f : progress / inProgressCount;
        }
    }
    
}