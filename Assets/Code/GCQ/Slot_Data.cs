namespace GCQ
{
    public class Slot_Data
    {
        public Slot_Type slot_type;
        public Unity.Mathematics.int2 id;
        public Human_Pawn human = null;
        public Human_Pawn bed = null;
        public Goblin_Pawn goblin = null;
        public Goblin_Pawn prev_goblin = null;
        public bool is_empty = true;
        public bool is_busy_spawning_or_dying = false;
    }
}