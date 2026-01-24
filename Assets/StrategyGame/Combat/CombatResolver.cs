namespace StrategyGame.Combat {
    public class CombatOutcome {
        public bool Hit;
        public bool Crit;
        public int DamageDealt;
        public bool DefenderDied;
        
        public bool CounterOccurs;
        public bool CounterHit;
        public bool CounterCrit;
        public int CounterDamageDealt;
        public bool AttackerDied;
    }

    public class CombatStats {
        public int HP;
        public int Attack;
        public int Defense;
        public int Speed;
        public int Accuracy;
        public int Resistance;
        public int Evasion;
    }
    
    public static class CombatResolver {
        public static CombatOutcome SimulateAttack() {
            CombatOutcome result = new CombatOutcome();
            return result;
        }
    }
}
