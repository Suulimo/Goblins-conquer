using System.Collections.Generic;
using UniRx;
using Unity.Mathematics;
using UnityEngine;

namespace GCQ
{
    public struct Swap_Info
    {
        public Color color;
        public Sprite sprite;
        public bool flip_x;
    }


    public class Battle_Scope
    {
        public Battle_Scope_Data data;

        public SortedDictionary<int2, Slot_Data> human_slot_group;
        public SortedDictionary<int2, Slot_Data> goblin_slot_group;
        public SortedDictionary<int2, Slot_Data> bed_slot_group;

        public Dictionary<Slot_Type, SortedDictionary<int2, Slot_Data>> slot_group;

        public Dictionary<(Slot_Type, int2), Slot_Data> slot_state_look_up;

        public void Init_Scope() {
            data = new Battle_Scope_Data();

            slot_group = new () {
                [Slot_Type.A] = goblin_slot_group = new SortedDictionary<int2, Slot_Data>(new Int2_Comparer()),
                [Slot_Type.B] = human_slot_group = new SortedDictionary<int2, Slot_Data>(new Int2_Comparer()),
                [Slot_Type.C] = bed_slot_group = new SortedDictionary<int2, Slot_Data>(new Int2_Comparer()),
            };

            slot_state_look_up = new Dictionary<(Slot_Type, int2), Slot_Data>();
        }

        public Slot_Data Add_Slot_Data(Slot_Type type, int2 id) {
            SortedDictionary<int2, Slot_Data> group = slot_group[type];

            var b = new Slot_Data() { id = id, slot_type = type };
            group[id] = b;
            slot_state_look_up.Add((type, id), b);

            return b;
        }
    }
}

