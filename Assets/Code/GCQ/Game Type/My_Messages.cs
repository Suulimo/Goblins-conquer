namespace GCQ
{

    public struct Human_Spawned
    {
        public Unity.Mathematics.int2 slot_id;
        public Human_Spec human_data;
    }

    public struct Goblin_Spawned
    {
        public Unity.Mathematics.int2 slot_id;
        public Goblin_Spec goblin_data;
    }

    public struct Bed_Spawned
    {
        public Unity.Mathematics.int2 slot_id;
        public Human_Spec human_data;
    }

    public struct Goblin_Attack_Human
    {
        public Unity.Mathematics.int2 goblin_slot_id;
        public Unity.Mathematics.int2 human_slot_id;
        public Goblin_Spec goblin_data;
    }

    public struct Human_Attack_Goblin
    {
        public Unity.Mathematics.int2 human_slot_id;
        public Unity.Mathematics.int2 goblin_slot_id;
        public Goblin_Spec goblin_data;
    }
}
