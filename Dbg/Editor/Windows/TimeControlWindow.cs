using UnityEditor;
using UnityEngine;

namespace MisterGames.Dbg.Editor.Windows {
    
    public class TimeControlWindow : EditorWindow {

        private const int FpsMax = 240;
        
        [MenuItem("MisterGames/Tools/Time Control")]
        private static void ShowWindow() {
            var window = GetWindow<TimeControlWindow>();
            window.titleContent = new GUIContent("Time Control");
            window.Show();
        }

        private void OnGUI() {
            TimeScaleGui();
            FramerateGui();
        }

        private static void TimeScaleGui() {
            EditorGUILayout.BeginHorizontal();
            
            Time.timeScale = EditorGUILayout.Slider(new GUIContent("Time scale"), Time.timeScale, 0f, 10f);
            
            if (GUILayout.Button("0", EditorStyles.miniButtonLeft)) Time.timeScale = 0f;
            if (GUILayout.Button(".1", EditorStyles.miniButtonMid)) Time.timeScale = 0.1f;
            if (GUILayout.Button(".25", EditorStyles.miniButtonMid)) Time.timeScale = 0.25f;
            if (GUILayout.Button(".5", EditorStyles.miniButtonMid)) Time.timeScale = 0.5f;
            if (GUILayout.Button("Normal", EditorStyles.miniButtonMid)) Time.timeScale = 1f;
            if (GUILayout.Button("2", EditorStyles.miniButtonMid)) Time.timeScale = 2f;
            if (GUILayout.Button("4", EditorStyles.miniButtonMid)) Time.timeScale = 4f;
            if (GUILayout.Button("10", EditorStyles.miniButtonRight)) Time.timeScale = 10f;
            
            EditorGUILayout.EndHorizontal();
        }
        
        private static void FramerateGui() {
            EditorGUILayout.BeginHorizontal();
            
            int fps = Application.targetFrameRate;
            int sliderValue = FpsToSlider(fps);
            int newSliderValue = EditorGUILayout.IntSlider(new GUIContent("Framerate"), sliderValue, 1, FpsMax);
            int newFps = SliderToFps(newSliderValue);

            Application.targetFrameRate = newFps;

            if (GUILayout.Button("30", EditorStyles.miniButtonLeft)) Application.targetFrameRate = 30;
            if (GUILayout.Button("60", EditorStyles.miniButtonMid)) Application.targetFrameRate = 60;
            if (GUILayout.Button("120", EditorStyles.miniButtonMid)) Application.targetFrameRate = 120;
            if (GUILayout.Button("Max", EditorStyles.miniButtonRight)) Application.targetFrameRate = 0;

            EditorGUILayout.EndHorizontal();
        }
        
        private static int FpsToSlider(int fps) => fps <= 0 ? FpsMax : fps;
        private static int SliderToFps(int slider) => slider >= FpsMax ? 0 : slider;
    }
    
}