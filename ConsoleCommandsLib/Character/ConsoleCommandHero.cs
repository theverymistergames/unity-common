using System;
using System.Globalization;
using System.Text;
using MisterGames.Character.Access;
using MisterGames.Character.Spawn;
using MisterGames.Common.Lists;
using MisterGames.Common.Pooling;
using MisterGames.Dbg.Console.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.ConsoleCommandsLib {

    [Serializable]
    public sealed class ConsoleCommandHero : IConsoleCommand {

        private const string SPAWN_POINT_ZERO_NAME = "World zero";

        [SerializeField] private GameObject _heroPrefab;

        public string Name => "hero";
        public string Description => "hero actions";

        IConsoleCommandResult IConsoleCommand.Process(string[] args) {
            if (args.IsEmpty()) return Usage();
            return args[0] switch {
                "spawns" => SpawnList(args),
                "spawn" => ExecuteSpawn(args),
                "teleport" => ExecuteTeleport(args),
                _ => Usage()
            };
        }

        private IConsoleCommandResult SpawnList(string[] args) {
            if (args.Length != 1) return Usage();
            
            var spawns = Object.FindObjectsOfType<CharacterSpawnPoint>();
            if (spawns.IsEmpty()) {
                return ConsoleCommandResults.Instant($"No character spawn points found");
            }

            return ConsoleCommandResults.Instant($"{SpawnPointListToText(spawns)}");
        }

        private IConsoleCommandResult ExecuteSpawn(string[] args) {
            if (args.Length != 2) return Usage();
            
            var spawnPoints = Object.FindObjectsOfType<CharacterSpawnPoint>();
            if (spawnPoints.IsEmpty()) {
                return ConsoleCommandResults.Instant("No character spawn points found");
            }

            string spawnPointName = SPAWN_POINT_ZERO_NAME;
            var spawnPosition = Vector3.zero;

            if (int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int inputIndex)) {
                int spawnPointIndex = inputIndex - 1;

                if (spawnPointIndex >= spawnPoints.Length) {
                    return ConsoleCommandResults.Instant(
                        $"Spawn point with index {inputIndex} not found.\n" +
                        $"Existent spawn points:\n{SpawnPointListToText(spawnPoints)}"
                    );
                }

                if (spawnPointIndex >= 0) {
                    var spawnPoint = spawnPoints[spawnPointIndex];

                    spawnPosition = spawnPoint.transform.position;
                    spawnPointName = spawnPoint.name;
                }
            }
            else {
                string spawnName = args[1];
                CharacterSpawnPoint spawnPoint = null;
                for (int i = 0; i < spawnPoints.Length; i++) {
                    var point = spawnPoints[i];
                    if (point.name != spawnName) continue;

                    spawnPoint = point;
                    break;
                }

                if (spawnPoint == null) {
                    return ConsoleCommandResults.Instant(
                        $"Spawn point with name '{spawnName}' not found.\n" +
                        $"Existent spawn points:\n{SpawnPointListToText(spawnPoints)}"
                    );
                }

                spawnPosition = spawnPoint.transform.position;
                spawnPointName = spawnPoint.name;
            }

            var access = Object.FindObjectOfType<CharacterAccess>();
            if (access == null) {
                var newHeroInstance = PrefabPool.Instance.TakeActive(_heroPrefab);
                access = newHeroInstance.GetComponent<CharacterAccess>();
            }

            if (access == null) {
                return ConsoleCommandResults.Instant(
                    $"Character with {nameof(CharacterAccess)} component not found on the scene and in prefabs."
                );
            }

            access.SetPosition(spawnPosition);

            return ConsoleCommandResults.Instant(
                $"Character {access.gameObject.name} was respawned at point [{spawnPointName} :: {spawnPosition}]"
            );
        }
        
        private IConsoleCommandResult ExecuteTeleport(string[] args) {
            int length = args.Length;
            if (length != 4 ||
                !float.TryParse(args[length - 3], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) ||
                !float.TryParse(args[length - 2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) ||
                !float.TryParse(args[length - 1], NumberStyles.Float, CultureInfo.InvariantCulture, out float z)) 
            {
                return Usage();
            }

            var access = Object.FindObjectOfType<CharacterAccess>();
            if (access == null) {
                var newHeroInstance = PrefabPool.Instance.TakeActive(_heroPrefab);
                access = newHeroInstance.GetComponent<CharacterAccess>();
            }

            if (access == null) {
                return ConsoleCommandResults.Instant(
                    $"Character with {nameof(CharacterAccess)} component not found " +
                    $"on scene to teleport and in prefabs to spawn new instance."
                );
            }

            var position = new Vector3(x, y, z);
            access.SetPosition(position);

            return ConsoleCommandResults.Instant(
                $"Character {access.gameObject.name} was teleported to [{position}]"
            );
        }

        private static string SpawnPointListToText(CharacterSpawnPoint[] spawns) {
            var builder = new StringBuilder();
            builder.Append($"[0] World zero: {Vector3.zero}");

            for (int i = 0; i < spawns.Length; i++) {
                var spawn = spawns[i];
                string item = $"[{i + 1}] {spawn.gameObject.name} : {spawn.transform.position}";
                
                if (i < spawns.Length - 1) builder.AppendLine(item);
                else builder.Append(item);
            }
            return builder.ToString();
        }

        private IConsoleCommandResult Usage() => ConsoleCommandResults.Instant(
            $"{Name} command usage:\n" +
            $"- {Name} spawns,\n" +
            $"- {Name} spawn spawn_point_name,\n" +
            $"- {Name} spawn 1 (index of a spawn point listed by '{Name} spawns' command),\n" +
            $"- {Name} teleport 0.5 0.5 0.5"
        );
    }
    
}
