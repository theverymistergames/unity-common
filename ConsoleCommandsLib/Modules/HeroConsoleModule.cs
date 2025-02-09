using System;
using MisterGames.Actors;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using MisterGames.Common.Pooling;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Input.Bindings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public class HeroConsoleModule : IConsoleModule {

        [SerializeField] private GameObject _heroPrefab;

        private const string SPAWN_POINT_ZERO_NAME = "World zero";

        public ConsoleRunner ConsoleRunner { get; set; }
        
        [ConsoleCommand("hero/spawni")]
        [ConsoleCommandHelp("spawn character at spawn point with index as in hero/spawns output")]
        public void SpawnAtPointByIndex(int index) {
            var spawnPoints = GetSpawnPoints();
            
            string spawnPointName = SPAWN_POINT_ZERO_NAME;
            var spawnPosition = Vector3.zero;
            
            int spawnPointIndex = index - 1;
            if (index < 0 || spawnPointIndex >= spawnPoints.Length) {
                ConsoleRunner.AppendLine($"Spawn point with index {index} not found. See hero/spawns");
                return;
            }

            if (spawnPointIndex >= 0) {
                var spawnPoint = spawnPoints[spawnPointIndex];

                spawnPosition = spawnPoint.transform.position;
                spawnPointName = spawnPoint.name;
            }

            SpawnHero(spawnPosition, spawnPointName);
        }

        [ConsoleCommand("hero/spawn")]
        [ConsoleCommandHelp("spawn character at spawn point with specified name")]
        public void SpawnAtPointByName(string spawnPointName) {
            var spawnPoints = GetSpawnPoints();
            var spawnPosition = Vector3.zero;

            CharacterSpawnPoint spawnPoint = null;
            for (int i = 0; i < spawnPoints.Length; i++) {
                var point = spawnPoints[i];
                if (point.name != spawnPointName) continue;

                spawnPoint = point;
                break;
            }

            if (spawnPoint == null && spawnPointName != SPAWN_POINT_ZERO_NAME) {
                ConsoleRunner.AppendLine($"Spawn point {spawnPointName} is not found. See hero/spawns");
                return;
            }

            if (spawnPoint != null) {
                spawnPosition = spawnPoint.transform.position;
                spawnPointName = spawnPoint.name;
            }

            SpawnHero(spawnPosition, spawnPointName);
        }

        [ConsoleCommand("hero/spawns")]
        [ConsoleCommandHelp("print character spawn points in current scene")]
        public void PrintSpawnList() {
            ConsoleRunner.AppendLine("Spawn points:");
            ConsoleRunner.AppendLine($"[0] {SPAWN_POINT_ZERO_NAME}");

            var spawnPoints = GetSpawnPoints();
            for (int i = 0; i < spawnPoints.Length; i++) {
                var spawn = spawnPoints[i];
                ConsoleRunner.AppendLine($"[{i + 1}] {spawn.gameObject.name} : {spawn.transform.position}");
            }
        }
        
        [ConsoleCommand("hero/select")]
        [ConsoleCommandHelp("select hero gameobject in current scene")]
        public void SelectHero() {
            var character = Object.FindFirstObjectByType<MainCharacter>();
            
            if (character == null) {
                ConsoleRunner.AppendLine($"No prefab with {nameof(MainCharacter)} component attached found on scene.");
                return;
            }

#if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = character.gameObject;
            
            var type = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var window = UnityEditor.EditorWindow.GetWindow(type);
            var method = type.GetMethod("SetExpandedRecursive");
            
            method!.Invoke(window, new object[] { character.gameObject.GetInstanceID(), true });

            int childCount = character.transform.childCount;
            for (int i = 0; i < childCount; i++) {
                method!.Invoke(window, new object[] { character.transform.GetChild(i).gameObject.GetInstanceID(), false });   
            }
            
            UnityEditor.EditorGUIUtility.PingObject(character.gameObject);
            UnityEditor.SceneView.lastActiveSceneView.FrameSelected();
#endif
        }
        
        [ConsoleHotkey("hero/select", KeyBinding.H, ShortcutModifiers.Control)]
        public void SelectHeroHotKey() { }

        [ConsoleHotkey("hero/spawni 0", KeyBinding.A0, ShortcutModifiers.Alt)]
        public void SpawnAtPointByIndex0() { }

        [ConsoleHotkey("hero/spawni 1", KeyBinding.A1, ShortcutModifiers.Alt)]
        public void SpawnAtPointByIndex1() { }
        
        [ConsoleHotkey("hero/spawni 2", KeyBinding.A2, ShortcutModifiers.Alt)]
        public void SpawnAtPointByIndex2() { }
        
        [ConsoleHotkey("hero/spawni 3", KeyBinding.A3, ShortcutModifiers.Alt)]
        public void SpawnAtPointByIndex3() { }
        
        [ConsoleHotkey("hero/spawni 4", KeyBinding.A4, ShortcutModifiers.Alt)]
        public void SpawnAtPointByIndex4() { }
        
        [ConsoleHotkey("hero/spawni 5", KeyBinding.A5, ShortcutModifiers.Alt)]
        public void SpawnAtPointByIndex5() { }
        
        [ConsoleHotkey("hero/spawni 6", KeyBinding.A6, ShortcutModifiers.Alt)]
        public void SpawnAtPointByIndex6() { }
        
        [ConsoleHotkey("hero/spawni 7", KeyBinding.A7, ShortcutModifiers.Alt)]
        public void SpawnAtPointByIndex7() { }
        
        [ConsoleHotkey("hero/spawni 8", KeyBinding.A8, ShortcutModifiers.Alt)]
        public void SpawnAtPointByIndex8() { }
        
        [ConsoleHotkey("hero/spawni 9", KeyBinding.A9, ShortcutModifiers.Alt)]
        public void SpawnAtPointByIndex9() { }

        private CharacterSpawnPoint[] GetSpawnPoints() {
            var spawnPoints = Object.FindObjectsByType<CharacterSpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Array.Sort(spawnPoints, (p0, p1) => p0.transform.GetInstanceID().CompareTo(p1.transform.GetInstanceID()));
            return spawnPoints;
        }
        
        private void SpawnHero(Vector3 position, string spawnPointName) {
            var access = Object.FindFirstObjectByType<MainCharacter>();
            if (access == null) {
                var newHeroInstance = PrefabPool.Main.Get(_heroPrefab);
                access = newHeroInstance.GetComponent<MainCharacter>();
            }

            if (access == null) {
                ConsoleRunner.AppendLine($"Character with {nameof(MainCharacter)} component not found on the scene and in prefabs.");
                return;
            }

            var actor = access.GetComponent<IActor>();
            actor.GetComponent<CharacterMotionPipeline>().Teleport(position, actor.Transform.rotation, preserveVelocity: false);

            ConsoleRunner.AppendLine($"Character {access.name} was respawned at point [{spawnPointName} :: {position}]");
        }
    }

}
