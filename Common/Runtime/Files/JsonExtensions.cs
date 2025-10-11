using System;
using System.IO;
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
        
        public static async UniTask<Result> WriteJsonIntoFile(object fileDto, string filePath, int bufferSize) {
            await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
            await using var sw = new StreamWriter(fs);
            
            try {
                await sw.WriteAsync(JsonUtility.ToJson(fileDto, prettyPrint: true));
                return new Result(Status.Success);
            }
            catch (IOException e) {
                Console.WriteLine(e);
                return new Result(Status.Error, message: e.Message);
            }
            finally {
                sw.Close();
                fs.Close();
            }
        }
        
        public static async UniTask<Result<T>> ReadJsonFromFile<T>(string filePath, int bufferSize) {
            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            using var sr = new StreamReader(fs);
            
            try {
                string json = await sr.ReadToEndAsync();
                var result = JsonUtility.FromJson<T>(json);

                return new Result<T>(Status.Success, result);
            }
            catch (IOException e) {
                Console.WriteLine(e);
                return new Result<T>(Status.Error, message: e.Message);
            }
            finally {
                sr.Close();
                fs.Close();
            }
        }
    }
    
}