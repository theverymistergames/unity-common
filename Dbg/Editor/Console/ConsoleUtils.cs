using System;
using System.Collections.Generic;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.MisterGames.Dbg.Editor.Console {
    
    public static class ConsoleUtils {
        
        public static IReadOnlyList<IConsoleModule> GetAllConsoleModules() {
            var allConsoleModules = new List<IConsoleModule>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++) {
                var assembly = assemblies[a];
                var types = assembly.GetTypes();

                for (int t = 0; t < types.Length; t++) {
                    var type = types[t];
                    if (type.IsAbstract || !type.GetInterfaces().Contains(typeof(IConsoleModule))) continue;

                    var module = (IConsoleModule) Activator.CreateInstance(type);
                    allConsoleModules.Add(module);
                }
            }

            return allConsoleModules;
        }
    }
    
}