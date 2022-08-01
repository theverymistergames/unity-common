using System;

namespace MisterGames.Common.Routines {
    
    public static class Jobs {

        public static void StartFrom(this IJob job, SingleJobHandler handler) {
            handler.Start(job);
        }
        
        public static JobSequence Do(IJob job) {
            return new JobSequence(job);
        }
            
        public static JobSequence Do(Action action) {
            return new JobSequence(Instant(action));
        }

        internal static IJob Instant(Action action = null) {
            return new InstantJob(action);
        }
    }
    
}
