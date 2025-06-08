using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using UnityEngine;

namespace MisterGames.ConsoleCommandsLib.Modules {
    
    public class VideoSettingsConsoleModule : IConsoleModule {
        
        public ConsoleRunner ConsoleRunner { get; set; }

        private readonly (int width, int height)[] _resolutions = {
            (426, 240),
            (1280, 720),
            (1920, 1080),
            (2560, 1440),
            (3840, 2160),
        };
        
        [ConsoleCommand("screen/reslist")]
        [ConsoleCommandHelp("resolution list")]
        public void PrintScreenResolutionVariants() {
            for (int i = 0; i < _resolutions.Length; i++) {
                var res = _resolutions[i];
                ConsoleRunner.AppendLine($"[{i}] {res.width} x {res.height}");
            }
        }
        
        [ConsoleCommand("screen/info")]
        [ConsoleCommandHelp("print screen resolution, vsync, fps")]
        public void PrintScreenInfo() {
            ConsoleRunner.AppendLine($"Screen resolution: {Screen.currentResolution.width} x {Screen.currentResolution.height}");
            ConsoleRunner.AppendLine($"VSync: {QualitySettings.vSyncCount}");
            ConsoleRunner.AppendLine($"Target FPS: {Application.targetFrameRate} (not used if VSync is enabled)");
        }
        
        [ConsoleCommand("screen/res")]
        [ConsoleCommandHelp("set resolution width and height")]
        public void SetResolution(int x, int y) {
            var resMin = _resolutions[0];

            x = Mathf.Max(resMin.width, x);
            y = Mathf.Max(resMin.height, y);
            
            Screen.SetResolution(x, y, Screen.fullScreenMode);
            
            PrintScreenInfo();
        }
        
        [ConsoleCommand("screen/resi")]
        [ConsoleCommandHelp("set resolution by index from screen/reslist")]
        public void SetResolutionIndex(int index) {
            if (index < 0 || index >= _resolutions.Length) {
                ConsoleRunner.AppendLine($"Invalid resolution index {index}, index should be in range 0..{_resolutions.Length - 1}.");
                return;
            }
            
            var res = _resolutions[index];
            SetResolution(res.width, res.height);
        }
        
        [ConsoleCommand("screen/setfps")]
        [ConsoleCommandHelp("set target frame rate")]
        public void SetFps(int fps) {
            if (fps <= 0) fps = -1;
            
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fps;
            
            ConsoleRunner.AppendLine($"Target frame rate set to {fps}, vsync set to 0 to support explicit fps");
            
            PrintScreenInfo();
        }
        
        [ConsoleCommand("screen/setvsync")]
        [ConsoleCommandHelp("set vsync count")]
        public void SetVsync(int vsync) {
            vsync = Mathf.Clamp(vsync, 0, 4);
            QualitySettings.vSyncCount = vsync;
            
            ConsoleRunner.AppendLine($"VSync set to {vsync}");
            
            PrintScreenInfo();
        }
    }
    
}