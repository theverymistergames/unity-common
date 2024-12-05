using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Logic.Damage {

    public readonly struct DamageInfo {
        
        public readonly float damage;
        public readonly bool mortal;
        public readonly IActor author;
        public readonly IActor victim;
        public readonly Vector3 point;
        
        public DamageInfo(
            IActor victim, 
            float damage, 
            bool mortal, 
            IActor author = null, 
            Vector3 point = default
        ) {
            this.damage = damage;
            this.mortal = mortal;
            this.author = author;
            this.victim = victim;
            this.point = point;
        }

        public static DamageInfo Empty(
            IActor victim,
            IActor author = default,
            Vector3 point = default
        ) {
            return new DamageInfo(victim, 0, mortal: false, author, point);
        }

        public DamageInfo WithVictim(IActor victim) => new(victim, damage, mortal, author, point);
        public DamageInfo WithDamage(int damage) => new(victim, damage, mortal, author, point);
        public DamageInfo WithMortal(bool mortal) => new(victim, damage, mortal, author, point);
        public DamageInfo WithAuthor(IActor author) => new(victim, damage, mortal, author, point);
        public DamageInfo WithPoint(Vector3 point) => new(victim, damage, mortal, author, point);

        public static implicit operator bool(DamageInfo info) {
            return info.damage > 0;
        }

        public override string ToString() {
            return $"{nameof(DamageInfo)}(victim {victim}, damage {damage}, mortal {mortal}, author {author})";
        }
    }
    
}