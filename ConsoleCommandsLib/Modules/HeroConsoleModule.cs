using MisterGames.Character.Access;
using MisterGames.Character.Spawn;
using MisterGames.Common.Pooling;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using UnityEngine;

namespace MisterGames.ConsoleCommandsLib.Modules {

    public class HeroConsoleModule : IConsoleModule {

        [SerializeField] private GameObject _heroPrefab;

        private const string SPAWN_POINT_ZERO_NAME = "World zero";

        [ConsoleCommand("hero/spawni")]
        [ConsoleCommandHelp("spawn character at spawn point with index as in hero/spawns output")]
        public void SpawnAtPointByIndex(int index) {
            var spawnPoints = Object.FindObjectsOfType<CharacterSpawnPoint>();

            string spawnPointName = SPAWN_POINT_ZERO_NAME;
            var spawnPosition = Vector3.zero;

            int spawnPointIndex = index - 1;
            if (index < 0 || spawnPointIndex >= spawnPoints.Length) {
                ConsoleRunner.Instance.AppendLine($"Spawn point with index {index} not found. See hero/spawns");
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
            var spawnPoints = Object.FindObjectsOfType<CharacterSpawnPoint>();
            var spawnPosition = Vector3.zero;

            CharacterSpawnPoint spawnPoint = null;
            for (int i = 0; i < spawnPoints.Length; i++) {
                var point = spawnPoints[i];
                if (point.name != spawnPointName) continue;

                spawnPoint = point;
                break;
            }

            if (spawnPoint == null && spawnPointName != SPAWN_POINT_ZERO_NAME) {
                ConsoleRunner.Instance.AppendLine($"Spawn point {spawnPointName} not found. See hero/spawns");
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
            ConsoleRunner.Instance.AppendLine("Spawn points:");
            ConsoleRunner.Instance.AppendLine($"[0] {SPAWN_POINT_ZERO_NAME}");

            var spawnPoints = Object.FindObjectsOfType<CharacterSpawnPoint>();
            for (int i = 0; i < spawnPoints.Length; i++) {
                var spawn = spawnPoints[i];
                ConsoleRunner.Instance.AppendLine($"[{i + 1}] {spawn.gameObject.name} : {spawn.transform.position}");
            }
        }

        private void SpawnHero(Vector3 position, string spawnPointName) {
            var access = Object.FindObjectOfType<CharacterAccess>();
            if (access == null) {
                var newHeroInstance = PrefabPool.Instance.TakeActive(_heroPrefab);
                access = newHeroInstance.GetComponent<CharacterAccess>();
            }

            if (access == null) {
                ConsoleRunner.Instance.AppendLine($"Character with {nameof(CharacterAccess)} component not found on the scene and in prefabs.");
                return;
            }

            access.SetPosition(position);
            ConsoleRunner.Instance.AppendLine($"Character {access.name} was respawned at point [{spawnPointName} :: {position}]");
        }
    }

}
