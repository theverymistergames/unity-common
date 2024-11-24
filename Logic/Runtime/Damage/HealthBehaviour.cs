using MisterGames.Actors;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Logic.Damage {
    
    public sealed class HealthBehaviour : MonoBehaviour, IActorComponent {

        [SerializeField] private bool _restoreFullHealthOnAwake;

        public delegate void DamageCallback(HealthBehaviour health, DamageInfo info);
        public delegate void HealthCallback(HealthBehaviour health);
        
        public event DamageCallback OnDamage = delegate { };
        public event HealthCallback OnDeath = delegate { };
        public event HealthCallback OnRestoreHealth = delegate { };
        
        public float Health { get; private set; }
        public bool IsAlive => Health > 0f;
        public bool IsDead => Health <= 0f;
        
        private HealthData _healthData;

        void IActorComponent.OnAwake(IActor actor) {
            if (_restoreFullHealthOnAwake) RestoreFullHealth(); 
        }

        void IActorComponent.OnSetData(IActor actor) {
            _healthData = actor.GetData<HealthData>();
        }

        public void RestoreFullHealth() {
            float oldHealth = Health;
            Health = _healthData.health;
            
            if (Health <= oldHealth) return;
            
            OnRestoreHealth.Invoke(this);
        }

        public DamageInfo Kill(bool notifyDamage = true) {
            float oldHealth = Health;
            Health = 0f;
            
            float damageTotal = oldHealth - Health;  
            var info = new DamageInfo(damageTotal, mortal: true);

            if (oldHealth > 0f) {
                if (notifyDamage) OnDamage.Invoke(this, info);
                OnDeath.Invoke(this);   
            }
            
            return info;
        }
        
        public DamageInfo TakeDamage(float damage) {
            float oldHealth = Health;
            Health = Mathf.Max(0f, Health - damage);
            
            float damageTotal = oldHealth - Health;  
            bool mortal = Health <= 0;
            
            var info = new DamageInfo(damageTotal, mortal);

            if (oldHealth > 0f) {
                OnDamage.Invoke(this, info);
                if (mortal) OnDeath.Invoke(this);   
            }
            
            return info;
        }

#if UNITY_EDITOR
        [Button] private void RestoreHealth() => RestoreFullHealth();
        [Button] private void KillHealth() => Kill(notifyDamage: true);
#endif
    }
    
}