using System;
using System.Collections.Generic;
using MisterGames.Input.Bindings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    public static class InputServices {
        
        public static IInputMapper Mapper { get; internal set; }
        public static IInputBlockService Blocks { get; internal set; }
        public static IInputBindingHelper BindingHelper { get; internal set; }

#if UNITY_EDITOR
        private const string RunPlayerUpdatesInEditModeFeature = "RUN_PLAYER_UPDATES_IN_EDIT_MODE";
        
        private static readonly HashSet<int> _enabledPlayerUpdatesInEditModeSources = new();

        public static void EnableInputInEditModeForSource(object source, bool enable) {
            if (Application.isPlaying) return;
            
            if (enable) {
                _enabledPlayerUpdatesInEditModeSources.Add(source.GetHashCode());
                
                Mapper ??= CreateInputStorage();
                Blocks ??= CreateInputBlockService(Mapper);
                BindingHelper ??= CreateInputBindingHelper();
                
                InputSystem.settings.SetInternalFeatureFlag(RunPlayerUpdatesInEditModeFeature, true);
                return;
            }

            _enabledPlayerUpdatesInEditModeSources.Remove(source.GetHashCode());
            if (_enabledPlayerUpdatesInEditModeSources.Count > 0) return;
            
            DisableInputInEditModeAndClearSources();
        }

        internal static void DisableInputInEditModeAndClearSources() {
            _enabledPlayerUpdatesInEditModeSources.Clear();
            
            InputSystem.settings.SetInternalFeatureFlag(RunPlayerUpdatesInEditModeFeature, false);

            if (Application.isPlaying) return;
            
            DisposeServices();
        }

        internal static void DisposeServices() {
            (Mapper as IDisposable)?.Dispose();
            (Blocks as IDisposable)?.Dispose();
            (BindingHelper as IDisposable)?.Dispose();
                
            Mapper = null;
            Blocks = null;
            BindingHelper = null;
        }

        private static IInputMapper CreateInputStorage() {
            var storage = new InputMapper();
            storage.Initialize(InputSystem.actions);
            return storage;
        }

        private static IInputBlockService CreateInputBlockService(IInputMapper mapper) {
            var blocks = new InputBlockService();
            blocks.Initialize(mapper);
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