﻿using System;
using MisterGames.Common.Attributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
#endif

namespace MisterGames.Scenes.Core {

    public sealed class ScenesStorage : MisterGames.Common.Data.ScriptableSingleton<ScenesStorage> {

#if UNITY_EDITOR
        [SerializeField] private bool _enablePlayModeStartSceneOverride = true;
#endif

        [SerializeField] private SceneReference _sceneRoot;
        [SerializeField] [ReadOnly] private string _sceneStart;
        [SerializeField] [BeginReadOnlyGroup] private string[] _sceneNames;

        public string[] SceneNames => _sceneNames;
        public string SceneRoot => _sceneRoot.scene;
        public string SceneStart {
            get => _sceneStart; 
            set => _sceneStart = value;
        }

        protected override void OnSingletonInstanceLoaded() {
#if UNITY_EDITOR
            RefreshSceneNames();
            SetActiveSceneAsStartSceneIfNotSet();
            TrySetSceneRootIfNotSet();
            SaveAsset();

            TrySetPlaymodeStartScene(_sceneRoot.scene);
#endif
        }

#if UNITY_EDITOR
        private void OnValidate() {
            TrySetPlaymodeStartScene(_sceneRoot.scene);
        }

        public void Refresh() {
            RefreshSceneNames();
            SaveAsset();
        }

        private void RefreshSceneNames() {
            var assets = GetAllSceneAssets();
            _sceneNames = assets.Select(asset => asset.name).ToArray();
        }
        
        private void SetActiveSceneAsStartSceneIfNotSet() {
            if (!string.IsNullOrEmpty(_sceneStart)) return;
            _sceneStart = SceneManager.GetActiveScene().name;
        }

        private void TrySetSceneRootIfNotSet() {
            if (!string.IsNullOrEmpty(_sceneRoot.scene)) return;

            for (int i = 0; i < _sceneNames.Length; i++) {
                string sceneName = _sceneNames[i];
                
                bool hasWordRoot = sceneName.IndexOf("root", StringComparison.OrdinalIgnoreCase) >= 0;
                bool hasWordGlobal = sceneName.IndexOf("global", StringComparison.OrdinalIgnoreCase) >= 0;
                bool hasWordSetup  = sceneName.IndexOf("setup", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!hasWordRoot && !hasWordGlobal && !hasWordSetup) continue;
                
                _sceneRoot.scene = sceneName;
                return;
            } 
            
            _sceneRoot.scene = SceneManager.GetActiveScene().name;
        }
        
        private void TrySetPlaymodeStartScene(string sceneName) {
            if (!_enablePlayModeStartSceneOverride) {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            var currentPlaymodeStartScene = EditorSceneManager.playModeStartScene;
            if (currentPlaymodeStartScene != null && currentPlaymodeStartScene.name == sceneName) {
                return;
            }
            
            var sceneRootAsset = GetAllSceneAssets().FirstOrDefault(asset => asset.name == sceneName);
            if (sceneRootAsset == null) return;

            EditorSceneManager.playModeStartScene = sceneRootAsset;
        }

        private static IEnumerable<SceneAsset> GetAllSceneAssets() => AssetDatabase
            .FindAssets($"a:assets t:{nameof(SceneAsset)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => !string.IsNullOrEmpty(path))
            .Select(AssetDatabase.LoadAssetAtPath<SceneAsset>)
            .Where(asset => asset != null);

        private void SaveAsset() {
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
