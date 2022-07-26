using System.Collections.Generic;
using MisterGames.Common.Strings;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace MisterGames.Common.Editor.Windows {
    
    public class MisterGamesPackagesWindow : EditorWindow {
        
        private static readonly List<PackageInfo> _packages = new List<PackageInfo> {
            CreatePackageInfo("bezier"),
            CreatePackageInfo("blueprints"),
            CreatePackageInfo("character"),
            CreatePackageInfo("common"),
            CreatePackageInfo("dbg"),
            CreatePackageInfo("fsm"),
            CreatePackageInfo("input"),
            CreatePackageInfo("interact"),
            CreatePackageInfo("scenario"),
            CreatePackageInfo("scenes"),
            CreatePackageInfo("view"),
            
            CreatePackageInfo("blueprintlib", "Blueprint Lib"),
            CreatePackageInfo("consolecommandslib", "Console Lib"),
        };
        
        private const string RepoPath = "https://gitlab.com/theverymistergames/";
        private const string NamespacePrefix = "com.mistergames.";
        
        private const int LabelHeight = 20;
        private const int LabelWidth = 100;
        private const int Margin = 4;
        
        private Request _currentRequest;
        private string _stateText = "Ready";

        [MenuItem("MisterGames/Packages")]
        private static void ShowWindow() {
            var window = GetWindow<MisterGamesPackagesWindow>();
            window.titleContent = new GUIContent("MisterGames Packages");
            
            var pos = window.position;
            pos.width = 400;
            pos.height = 800;
            
            window.Show();
        }
        
        private void OnGUI() {
            var pos = new Rect { x = Margin, y = Margin, width = LabelWidth, height = LabelHeight };
            GUI.Label(pos, "Packages");

            pos.y += LabelHeight;
            pos.width = 300;
            GUI.Label(pos, $"Repository: {RepoPath}");

            pos.y += Margin + LabelHeight;
            pos.width = LabelWidth;
            foreach (var info in _packages) {
                CreatePackageGui(pos, info);
                pos.y += Margin + LabelHeight;
            }

            pos.y += Margin * 4;
            pos.width = 500;
            GUI.Label(pos, _stateText);
        }

        private void CreatePackageGui(Rect pos, PackageInfo info) {
            pos.x += Margin * 4;
            GUI.Label(pos, $"- {info.displayName}");
            
            GUI.enabled = _currentRequest == null || _currentRequest.IsCompleted;
            
            pos.x += Margin + LabelWidth;
            if (GUI.Button(pos, "Add or update")) RequestOperation(Operation.InstallOrUpdate, info);
            
            pos.x += Margin + LabelWidth;
            if (GUI.Button(pos, "Remove")) RequestOperation(Operation.Remove, info);
            
            GUI.enabled = true;
        }
        
        private void RequestOperation(Operation operation, PackageInfo info) {
            if (_currentRequest != null && !_currentRequest.IsCompleted) return;
            
            _stateText = $"{GetOperationVerb(operation)} package {info.name} ...";
            _currentRequest = CreateRequest(operation, info);
            
            // ReSharper disable once DelegateSubtraction
            EditorApplication.update -= UpdateRequestProgress;
            EditorApplication.update += UpdateRequestProgress;
        }
        
        private void UpdateRequestProgress() {
            if (_currentRequest == null || !_currentRequest.IsCompleted) return;
            
            switch (_currentRequest) {
                case AddRequest addRequest:
                    if (addRequest.Status == StatusCode.Success) {
                        _stateText = $"Package installed: {addRequest.Result.name}";
                    }
                    else if (addRequest.Status >= StatusCode.Failure) {
                        _stateText = $"Package not installed";
                        Debug.LogWarning($"MisterGames Packages: package not installed: {addRequest.Error.message}");
                    }
                    break;
                
                case RemoveRequest removeRequest:
                    if (removeRequest.Status == StatusCode.Success) {
                        _stateText = $"Package removed: {removeRequest.PackageIdOrName}";
                    }
                    else if (removeRequest.Status >= StatusCode.Failure) {
                        _stateText = $"Package not removed: {removeRequest.PackageIdOrName}";
                        Debug.LogWarning($"MisterGames Packages: package {removeRequest.PackageIdOrName} not removed: {removeRequest.Error.message}");
                    }
                    break;
            }
            
            // ReSharper disable once DelegateSubtraction
            EditorApplication.update -= UpdateRequestProgress;
            _currentRequest = null;
        }

        private static string GetOperationVerb(Operation operation) {
            switch (operation) {
                case Operation.InstallOrUpdate:
                    return "Updating";
                
                case Operation.Remove:
                    return "Removing";
            }

            return "<unknown operation>";
        } 
        
        private static Request CreateRequest(Operation operation, PackageInfo info) {
            if (operation == Operation.InstallOrUpdate) return Client.Add(info.gitUrl);
            return Client.Remove(info.name);
        }

        private static string CreateGitUrl(string package) => $"{RepoPath}{package}.git";
        private static string CreateName(string package) => $"{NamespacePrefix}{package}";
        private static string CreateDisplayName(string package) => $"{package.UpperFirstLetter()}";
        
        private static PackageInfo CreatePackageInfo(string package, string displayName = null) {
            displayName ??= CreateDisplayName(package);
            return new PackageInfo {
                name = CreateName(package), 
                displayName = displayName, 
                gitUrl = CreateGitUrl(package)
            };
        }

        private struct PackageInfo {
            public string name;
            public string displayName;
            public string gitUrl;
        }
        
        private enum Operation {
            InstallOrUpdate,
            Remove,
        }
        
    }
}