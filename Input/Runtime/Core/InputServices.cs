using System;
using System.Collections.Generic;
using MisterGames.Input.Bindings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    public static class InputServices {
        
        public static IInputStorage Storage { get; internal set; }
        public static IInputBlockService Blocks { get; internal set; }
        public static IInputBindingHelper BindingHelper { get; internal set; }

#if UNITY_EDITOR
        private const string RunPlayerUpdatesInEditModeFeature = "RUN_PLAYER_UPDATES_IN_EDIT_MODE";
        
        private static readonly HashSet<int> _enabledPlayerUpdatesInEditModeSources = new();

        public static void EnableInputInEditModeForSource(object source, bool enable) {
            if (Application.isPlaying) return;
            
            if (enable) {
                _enabledPlayerUpdatesInEditModeSources.Add(source.GetHashCode());
                
                Storage ??= CreateInputStorage();
                Blocks ??= CreateInputBlockService(Storage);
                BindingHelper ??= CreateInputBindingHelper();
                
                InputSystem.settings.SetInternalFeatureFlag(RunPlayerUpdatesInEditModeFeature, true);
                return;
            }

            _enabledPlayerUpdatesInEditModeSources.Remove(source.GetHashCode());
            if (_enabledPlayerUpdatesInEditModeSources.Count > 0) return;
            
            DisableInputInEditModeAndClearSources();
        }

        public static void DisableInputInEditModeAndClearSources() {
            _enabledPlayerUpdatesInEditModeSources.Clear();
            
            if (Application.isPlaying) return;
            
            (Storage as IDisposable)?.Dispose();
            (Blocks as IDisposable)?.Dispose();
            (BindingHelper as IDisposable)?.Dispose();
                
            Storage = null;
            Blocks = null;
            BindingHelper = null;
            
            InputSystem.settings.SetInternalFeatureFlag(RunPlayerUpdatesInEditModeFeature, false);
        }

        private static IInputStorage CreateInputStorage() {
            var storage = new InputStorage();
            storage.Initialize();
            return storage;
        }

        private static IInputBlockService CreateInputBlockService(IInputStorage storage) {
            var blocks = new InputBlockService();
            blocks.Initialize(storage);
            return blocks;
        }

        private static IInputBindingHelper CreateInputBindingHelper() {
            var helper = new InputBindingHelper();
            helper.Initialize();
            return helper;
        }
#endif
    }
    
}