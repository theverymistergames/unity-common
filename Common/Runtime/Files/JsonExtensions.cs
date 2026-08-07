using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.Common.Files {

    public static class JsonExtensions {

        public readonly struct Result {

            public readonly Status status;
            public readonly string message;

            public Result(Status status, string message = null) {
                this.status = status;
                this.message = message;
            }
        }

        public readonly struct Result<T> {

            public readonly Status status;
            public readonly T value;
            public readonly string message;

            public Result(Status status, T value = default, string message = null) {
                this.status = status;
                this.value = value;
                this.message = message;
            }
        }

        public enum Status {
            Success,
            Error,
        }

        private const int RetryCount = 5;
        private const int RetryDelayMs = 50;
        private const string TempFileExtension = ".tmp";

        public static string SerializeJson(object fileDto) {
            return JsonUtility.ToJson(fileDto, prettyPrint: true);
        }

        public static UniTask<Result> WriteJsonIntoFile(object fileDto, string filePath, int bufferSize, object fileLock = null) {
            return WriteJsonIntoFile(SerializeJson(fileDto), filePath, bufferSize, fileLock);
        }

        public static async UniTask<Result> WriteJsonIntoFile(string json, string filePath, int bufferSize, object fileLock = null) {
            return await UniTask.RunOnThreadPool(() => WriteTextIntoFile(json, filePath, bufferSize, fileLock));
        }

        public static async UniTask<Result<T>> ReadJsonFromFile<T>(string filePath, int bufferSize, object fileLock = null) {
            var result = await UniTask.RunOnThreadPool(() => ReadTextFromFile(filePath, bufferSize, fileLock));

            if (result.status != Status.Success) {
                return new Result<T>(Status.Error, message: result.message);
            }

            try {
                return new Result<T>(Status.Success, JsonUtility.FromJson<T>(result.value));
            }
            catch (Exception e) {
                return new Result<T>(Status.Error, message: $"File at path {filePath} contains invalid json: {e.Message}");
            }
        }

        public static Result DeleteFile(string filePath, object fileLock = null) {
            if (fileLock == null) return DeleteFileInternal(filePath);

            lock (fileLock) {
                return DeleteFileInternal(filePath);
            }
        }

        private static Result WriteTextIntoFile(string json, string filePath, int bufferSize, object fileLock) {
            if (fileLock == null) return WriteTextIntoFileInternal(json, filePath, bufferSize);

            lock (fileLock) {
                return WriteTextIntoFileInternal(json, filePath, bufferSize);
            }
        }

        private static Result<string> ReadTextFromFile(string filePath, int bufferSize, object fileLock) {
            if (fileLock == null) return ReadTextFromFileInternal(filePath, bufferSize);

            lock (fileLock) {
                return ReadTextFromFileInternal(filePath, bufferSize);
            }
        }

        private static Result WriteTextIntoFileInternal(string json, string filePath, int bufferSize) {
            string tempPath = filePath + TempFileExtension;

            for (int i = 0; i <= RetryCount; i++) {
                try {
                    using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize))
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(json);
                    }

                    if (File.Exists(filePath)) File.Replace(tempPath, filePath, destinationBackupFileName: null);
                    else File.Move(tempPath, filePath);

                    return new Result(Status.Success);
                }
                catch (IOException) when (i < RetryCount) {
                    Thread.Sleep(RetryDelayMs);
                }
                catch (Exception e) {
                    DeleteTempFile(tempPath);
                    return new Result(Status.Error, message: e.Message);
                }
            }

            DeleteTempFile(tempPath);
            return new Result(Status.Error, message: $"File at path {filePath} is locked by another process");
        }

        private static Result<string> ReadTextFromFileInternal(string filePath, int bufferSize) {
            if (!File.Exists(filePath)) {
                return new Result<string>(Status.Error, message: $"File at path {filePath} is not found");
            }

            for (int i = 0; i <= RetryCount; i++) {
                try {
                    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize);
                    using var sr = new StreamReader(fs);

                    return new Result<string>(Status.Success, sr.ReadToEnd());
                }
                catch (IOException) when (i < RetryCount) {
                    Thread.Sleep(RetryDelayMs);
                }
                catch (Exception e) {
                    return new Result<string>(Status.Error, message: e.Message);
                }
            }

            return new Result<string>(Status.Error, message: $"File at path {filePath} is locked by another process");
        }

        private static Result DeleteFileInternal(string filePath) {
            for (int i = 0; i <= RetryCount; i++) {
                try {
                    if (File.Exists(filePath)) File.Delete(filePath);
                    return new Result(Status.Success);
                }
                catch (IOException) when (i < RetryCount) {
                    Thread.Sleep(RetryDelayMs);
                }
                catch (Exception e) {
                    return new Result(Status.Error, message: e.Message);
                }
            }

            return new Result(Status.Error, message: $"File at path {filePath} is locked by another process");
        }

        private static void DeleteTempFile(string tempPath) {
            try {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
            catch (Exception) {
                // Temp file is overwritten on the next write attempt anyway.
            }
        }
    }

}
