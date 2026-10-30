using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using MisterGames.Common.Pooling;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Input.Bindings;
using MisterGames.Logic.Damage;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public class HeroConsoleModule : IConsoleModule, IConsoleSuggestionProvider {

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

        [ConsoleCommand("hero/kill")]
        [ConsoleCommandHelp("kills hero if there is one on the currently opened scenes")]
        public void KillHero() {
            var hero = Object.FindFirstObjectByType<MainCharacter>();
            
            if (hero == null) {
                ConsoleRunner.AppendLine($"Character with {nameof(MainCharacter)} component not found on the scene and in prefabs.");
                return;
            }
            
            var actor = hero.GetComponent<IActor>();
            if (actor == null || !actor.TryGetComponent(out HealthBehaviour health)) {
                ConsoleRunner.AppendLine($"Character with {nameof(MainCharacter)}, {nameof(IActor)} and {nameof(HealthBehaviour)} components not found on the scene and in prefabs.");
                return;
            }
            
            var result = health.Kill(author: actor, point: actor.Transform.position, notifyDamage: true);

            ConsoleRunner.AppendLine($"Character {hero.name} was killed, damage info: {result}");
        }

        [ConsoleHotkey("hero/spawni 0", KeyBinding.Digit0, ShortcutModifiers.Shift)]
        public void SpawnAtPointByIndex0() { }

        [ConsoleHotkey("hero/spawni 1", KeyBinding.Digit1, ShortcutModifiers.Shift)]
        public void SpawnAtPointByIndex1() { }

        [ConsoleHotkey("hero/spawni 2", KeyBinding.Digit2, ShortcutModifiers.Shift)]
        public void SpawnAtPointByIndex2() { }

        [ConsoleHotkey("hero/spawni 3", KeyBinding.Digit3, ShortcutModifiers.Shift)]
        public void SpawnAtPointByIndex3() { }

        [ConsoleHotkey("hero/spawni 4", KeyBinding.Digit4, ShortcutModifiers.Shift)]
        public void SpawnAtPointByIndex4() { }

        [ConsoleHotkey("hero/spawni 5", KeyBinding.Digit5, ShortcutModifiers.Shift)]
        public void SpawnAtPointByIndex5() { }

        [ConsoleHotkey("hero/spawni 6", KeyBinding.Digit6, ShortcutModifiers.Shift)]
        public void SpawnAtPointByIndex6() { }

        [ConsoleHotkey("hero/spawni 7", KeyBinding.Digit7, ShortcutModifiers.Shift)]
        public void SpawnAtPointByIndex7() { }

        [ConsoleHotkey("hero/spawni 8", KeyBinding.Digit8, ShortcutModifiers.Shift)]
        public void SpawnAtPointByIndex8() { }

        [ConsoleHotkey("hero/spawni 9", KeyBinding.Digit9, ShortcutModifiers.Shift)]
        public void SpawnAtPointByIndex9() { }

        public void CollectSuggestions(string cmd, IReadOnlyList<string> args, ICollection<string> suggestions) {
            if (cmd != "hero/spawn" || args.Count != 1) return;

            var spawnPoints = GetSpawnPoints();
            string[] names = new string[spawnPoints.Length];

            for (int i = 0; i < spawnPoints.Length; i++) {
                names[i] = spawnPoints[i].name;
            }

            suggestions.AddMatches(names, args[0]);
        }

        private CharacterSpawnPoint[] GetSpawnPoints() {
            var spawnPoints = Object.FindObjectsByType<CharacterSpawnPoint>(FindObjectsInactive.Exclude);
            var paths = new Dictionary<CharacterSpawnPoint, List<int>>(spawnPoints.Length);

            for (int i = 0; i < spawnPoints.Length; i++) {
                var spawnPoint = spawnPoints[i];
                paths[spawnPoint] = GetHierarchyPath(spawnPoint.transform);
            }

            Array.Sort(spawnPoints, (p0, p1) => CompareHierarchyPaths(paths[p0], paths[p1]));
            return spawnPoints;
        }

        private static List<int> GetHierarchyPath(Transform transform) {
            var path = new List<int>();

            for (var t = transform; t != null; t = t.parent) {
                path.Add(t.GetSiblingIndex());
            }

            path.Add(GetSceneIndex(transform.gameObject.scene));
            path.Reverse();

            return path;
        }

        private static int GetSceneIndex(Scene scene) {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                if (SceneManager.GetSceneAt(i) == scene) return i;
            }

            return SceneManager.sceneCount;
        }

        private static int CompareHierarchyPaths(List<int> path0, List<int> path1) {
            int count = Mathf.Min(path0.Count, path1.Count);

            for (int i = 0; i < count; i++) {
                int result = path0[i].CompareTo(path1[i]);
                if (result != 0) return result;
            }

            return path0.Count.CompareTo(path1.Count);
        }

        private void SpawnHero(Vector3 position, string spawnPointName) {
            var hero = Object.FindAnyObjectByType<MainCharacter>();
            
            if (hero == null) {
                var newHeroInstance = PrefabPool.Main.Get(_heroPrefab);
                hero = newHeroInstance.GetComponent<MainCharacter>();
            }

            if (hero == null) {
                ConsoleRunner.AppendLine($"Character with {nameof(MainCharacter)} component not found on the scene and in prefabs.");
                return;
            }

            var actor = hero.GetComponent<IActor>();
            actor.GetComponent<CharacterMotionPipeline>().Teleport(position, actor.Transform.rotation, preserveVelocity: false);

            ConsoleRunner.AppendLine($"Character {hero.name} was respawned at point [{spawnPointName} :: {position}]");
        }
    }

}
