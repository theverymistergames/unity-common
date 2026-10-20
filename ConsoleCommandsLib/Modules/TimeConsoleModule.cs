using System;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Input.Bindings;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public sealed class TimeConsoleModule : IConsoleModule {

        public ConsoleRunner ConsoleRunner { get; set; }
        
        [ConsoleCommand("time/scale")]
        [ConsoleCommandHelp("set timescale with highest priority to override every other timescale sources")]
        public void SetTimescale(float timescale) {
            if (timescale.IsNearlyEqual(1f)) {
                Services.Get<ITimescaleSystem>().RemoveTimescale(ConsoleRunner);
            }
            else {
                Services.Get<ITimescaleSystem>().SetTimescale(ConsoleRunner, TimescalePriority.Debug, timescale);    
            }
            
            ConsoleRunner.AppendLine($"timescale is {Time.timeScale}");
        }
        
        [ConsoleCommand("time/scale")]
        [ConsoleCommandHelp("get timescale")]
        public void GetTimescale() {
            ConsoleRunner.AppendLine($"timescale is {Time.timeScale}");
        }
        
        [ConsoleCommand("time/pause")]
        [ConsoleCommandHelp("toggle playmode pause")]
        [ConsoleHotkey("time/pause", KeyBinding.F3)]
        public void SetPause() {
#if UNITY_EDITOR
            EditorApplication.isPaused = !EditorApplication.isPaused;
#endif
        }
    }

}
