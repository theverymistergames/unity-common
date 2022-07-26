using System;

namespace MisterGames.Common.Routines {
    
    public interface IJob {
        
        Action OnStop { set; }
        
        void Start();

        void Stop();
        
        void Pause();
        
        void Resume();
        
    }
    
}