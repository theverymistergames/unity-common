using System;

namespace MisterGames.Common.Localization {
    
    public static class ArgumentResolveExtensions {
        
        public static void ResolveArgs<T>(T args, int count, ref string value, Func<T, int, string> getValue) {
            switch (count) {
                case 0:
                    return;
                
                case 1:
                    value = string.Format(value, getValue.Invoke(args, 0));
                    break;
                        
                case 2:
                    value = string.Format(value, 
                        getValue.Invoke(args, 0),
                        getValue.Invoke(args, 1)
                    );
                    break;
                        
                case 3:
                    value = string.Format(value, 
                        getValue.Invoke(args, 0), 
                        getValue.Invoke(args, 1),
                        getValue.Invoke(args, 2)
                    );
                    break;
                    
                default:
                    object[] buffer = new object[count];
                    for (int i = 0; i < count; i++) {
                        buffer[i] = getValue.Invoke(args, i);
                    }

                    value = string.Format(value, buffer);
                    break;
            }
        }
    }
    
}