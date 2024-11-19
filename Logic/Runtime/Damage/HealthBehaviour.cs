using System;
using MisterGames.Actors;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Logic.Damage {
    
    public sealed class HealthBehaviour : MonoBehaviour, IActorComponent {

        public event Action<HealthBehaviour, DamageInfo> OnDamage = delegate { };
        public event Action<HealthBehaviour> OnDeath = delegate { };
        public event Action<HealthBehaviour> OnRestoreHealth = delegate { };
        
        public float Health { get; private set; }
        public bool IsAlive => Health > 0f;
        public bool IsDead => Health <= 0f;
        
        private HealthData _healthData;

        void IActorComponent.OnSetData(IActor actor) {
            _healthData = actor.GetData<HealthData>();
            RestoreFullHealth();
        }

        public void RestoreFullHealth() {
            Health = _healthData.health;
            OnRestoreHealth.Invoke(this);
        }

        public DamageInfo Kill(bool notifyDamage = true) {
            float oldHealth = Health;
            Health = 0f;
            
            float damageTotal = oldHealth - Health;  
            var info = new DamageInfo(damageTotal, mortal: true);
            
            if (notifyDamage) OnDamage.Invoke(this, info);
            
            OnDeath.Invoke(this);
            
            return info;
        }
        
        public DamageInfo TakeDamage(float damage) {
            float oldHealth = Health;
            Health = Mathf.Max(0f, Health - damage);
            
            float damageTotal = oldHealth - Health;  
            bool mortal = Health <= 0;
            
            var info = new DamageInfo(damageTotal, mortal);
            OnDamage.Invoke(this, info);
            
            if (mortal) OnDeath.Invoke(this);
            
            return info;
        }

#if UNITY_EDITOR
        [Button] private void RestoreHealth() => RestoreFullHealth();
        [Button] private void KillHealth() => Kill(notifyDamage: true);
#endif
    }
    
}