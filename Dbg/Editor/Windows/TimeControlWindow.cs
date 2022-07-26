using UnityEditor;
using UnityEngine;

namespace MisterGames.Dbg.Editor.Windows {
    
    public class TimeControlWindow : EditorWindow {
        
        [MenuItem("MisterGames/Time Control")]
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
            
            Time.timeScale = EditorGUILayout.Slider(new GUIContent("Time scale"), Time.timeScale, 0.01f, 2f);
            
            if (GUILayout.Button("0.1")) Time.timeScale = 0.1f;
            if (GUILayout.Button("1.0")) Time.timeScale = 1f;
            
            EditorGUILayout.EndHorizontal();
        }
        
        private static void FramerateGui() {
            EditorGUILayout.BeginHorizontal();
            
            int fps = Application.targetFrameRate;
            int sliderValue = FpsToSlider(fps);
            int newSliderValue = EditorGUILayout.IntSlider(new GUIContent("Framerate"), sliderValue, 1, 100);
            int newFps = SliderToFps(newSliderValue);

            Application.targetFrameRate = newFps;
            
            if (GUILayout.Button("Max")) Application.targetFrameRate = 0;
            if (GUILayout.Button("5")) Application.targetFrameRate = 5;
            
            EditorGUILayout.EndHorizontal();
        }
        
        private static int FpsToSlider(int fps) {
            return fps <= 0 ? 100 : fps;
        }
        
        private static int SliderToFps(int slider) => slider >= 100 ? 0 : slider;
        
    }
    
}