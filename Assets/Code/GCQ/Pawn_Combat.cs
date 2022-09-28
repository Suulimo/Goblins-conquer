using System.Collections.Generic;
using UniRx;

namespace GCQ
{
    public class Pawn_Combat
    {
        public bool is_busy;
        public IntReactiveProperty hp = new IntReactiveProperty();
        public FloatReactiveProperty attack_cycle = new FloatReactiveProperty();
        public int hp_max;
        public int attack_power;
        public int rank;
        public FloatReactiveProperty rage_time = new FloatReactiveProperty();
        public FloatReactiveProperty birth_drug_time = new FloatReactiveProperty();

        public HashSet<Unity.Mathematics.int2> melee_queue = new ();
        public HashSet<Unity.Mathematics.int2> movement_queue = new ();
    }
}