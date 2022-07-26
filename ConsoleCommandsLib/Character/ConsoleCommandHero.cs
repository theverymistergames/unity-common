using System.Globalization;
using System.Text;
using MisterGames.Character.Access;
using MisterGames.Character.Spawn;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Core;
using UnityEngine;

namespace MisterGames.ConsoleCommandsLib {

    public sealed class ConsoleCommandHero : IConsoleCommand {

        public string Name { get; } = "hero";
        public string Description { get; } = "hero actions";

        IConsoleCommandResult IConsoleCommand.Process(DeveloperConsoleRunner runner, string[] args) {
            if (args.IsEmpty()) return Usage();
            return args[0] switch {
                "spawns" => ExecuteSpawns(args),
                "spawn" => ExecuteSpawn(args),
                "teleport" => ExecuteTeleport(args),
                _ => Usage()
            };
        }

        private IConsoleCommandResult ExecuteSpawns(string[] args) {
            if (args.Length != 1) return Usage();
            
            var spawns = Object.FindObjectsOfType<CharacterSpawnPoint>();
            if (spawns.IsEmpty()) {
                return ConsoleCommandResults.Instant($"No character spawn points found");
            }

            return ConsoleCommandResults.Instant($"{GetSpawnList(spawns)}");
        }

        private IConsoleCommandResult ExecuteSpawn(string[] args) {
            if (args.Length != 2) return Usage();
            
            var spawns = Object.FindObjectsOfType<CharacterSpawnPoint>();
            if (spawns.IsEmpty()) {
                return ConsoleCommandResults.Instant($"No character spawn points found");
            }

            CharacterSpawnPoint spawn = null;
                
            if (int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int spawnIndex)) 
            {
                if (spawnIndex < 0 || spawnIndex >= spawns.Length) {
                    return ConsoleCommandResults.Instant(
                        $"Spawn point with index {spawnIndex} not found.\n" +
                        $"Existent spawn points:\n{GetSpawnList(spawns)}"
                    );
                }

                spawn = spawns[spawnIndex];
            }
            else {
                string spawnName = args[1];
                    
                for (int i = 0; i < spawns.Length; i++) {
                    var point = spawns[i];
                    if (point.name != spawnName) continue;

                    spawn = point;
                    break;
                }
                    
                if (spawn == null) {
                    return ConsoleCommandResults.Instant(
                        $"Spawn point with name '{spawnName}' not found.\n" +
                        $"Existent spawn points:\n{GetSpawnList(spawns)}"
                    );
                }
            }

            var access = Object.FindObjectOfType<CharacterAccess>();
            if (access == null) {
                return ConsoleCommandResults.Instant(
                    $"Character not found. " +
                    $"There must be a {nameof(CharacterAccess)} component on the character to teleport."
                );
            }

            var position = spawn.GetPoint();
            access.SetPosition(position);
                
            return ConsoleCommandResults.Instant(
                $"Character {access.gameObject.name} was teleported " +
                $"to spawn point {spawn.gameObject.name} " +
                $"at position {position}"
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
                return ConsoleCommandResults.Instant(
                    $"Character not found. " +
                    $"There must be a {nameof(CharacterAccess)} component on the character to teleport."
                );
            }

            var position = new Vector3(x, y, z);
            access.SetPosition(position);
 
            return ConsoleCommandResults.Instant(
                $"Character {access.gameObject.name} was teleported " +
                $"to position {position}"
            );
        }

        private static string GetSpawnList(CharacterSpawnPoint[] spawns) {
            var builder = new StringBuilder();
            for (int i = 0; i < spawns.Length; i++) {
                var spawn = spawns[i];
                string item = $"[{i}] {spawn.gameObject.name} : {spawn.GetPoint()}";
                
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