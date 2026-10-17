using System;
using System.Collections.Generic;
using MisterGames.Scenario.Events;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.ConsoleCommandsLib.Modules {

    [CreateAssetMenu(fileName = nameof(EventConsoleModuleData), menuName = "MisterGames/Console/" + nameof(EventConsoleModuleData))]
    public sealed class EventConsoleModuleData : ScriptableObject {

        [SerializeField] private EventDomain[] _eventDomains;

        public IReadOnlyList<EventDomain> EventDomains => _eventDomains ?? Array.Empty<EventDomain>();

        public const string ResourcesPath = nameof(EventConsoleModuleData);

        public static EventConsoleModuleData Load() {
            return Resources.Load<EventConsoleModuleData>(ResourcesPath);
        }

#if UNITY_EDITOR
        private const string ResourcesFolder = "Assets/Resources";
        private const string DefaultAssetPath = ResourcesFolder + "/" + ResourcesPath + ".asset";

        [MenuItem("MisterGames/Tools/Collect EventDomains for debug console")]
        private static void CollectEventDomains() {
            var data = Load();

            if (data == null) {
                if (!AssetDatabase.IsValidFolder(ResourcesFolder)) AssetDatabase.CreateFolder("Assets", "Resources");

                data = CreateInstance<EventConsoleModuleData>();
                AssetDatabase.CreateAsset(data, DefaultAssetPath);
            }

            data._eventDomains = AssetDatabase
                .FindAssets($"t:{nameof(EventDomain)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<EventDomain>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(domain => domain != null)
                .OrderBy(domain => domain.name)
                .ToArray();

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssetIfDirty(data);

            Selection.activeObject = data;
            EditorGUIUtility.PingObject(data);

            Debug.Log($"{nameof(EventConsoleModuleData)}: collected {data._eventDomains.Length} event domains into {AssetDatabase.GetAssetPath(data)}");
        }
#endif
    }

}
