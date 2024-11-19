namespace MisterGames.Logic.Damage {

    public readonly struct DamageInfo {
        
        public readonly float damage;
        public readonly bool mortal;
        
        public DamageInfo(float damage, bool mortal) {
            this.damage = damage;
            this.mortal = mortal;
        }

        public static implicit operator bool(DamageInfo info) {
            return info.damage > 0f;
        }
    }
    
}