using System;
using System.Globalization;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Core;
using UnityEngine;

namespace MisterGames.ConsoleCommandsLib {

    [Serializable]
    public sealed class ConsoleCommandSetPosition : IConsoleCommand {
        
        public string Name => "setposition";
        public string Description => "set position for gameobject found by name";

        IConsoleCommandResult IConsoleCommand.Process(string[] args) {
            int length = args.Length;
            
            if (length < 4 ||
                !float.TryParse(args[length - 3], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) || 
                !float.TryParse(args[length - 2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) || 
                !float.TryParse(args[length - 1], NumberStyles.Float, CultureInfo.InvariantCulture, out float z)) 
            {
                return ConsoleCommandResults.Instant($"{Name} command usage: {Name} gameobjectName 0 0 0");
            }

            string gameObjectName = string.Join(" ", args.Slice(0, length - 4));
            
            var gameObject = GameObject.Find(gameObjectName);
            if (gameObject == null) {
                return ConsoleCommandResults.Instant($"GameObject {gameObjectName} not found"); 
            }
            
            gameObject.transform.position = new Vector3(x, y, z);
            return ConsoleCommandResults.Empty;
        }
    }
}
