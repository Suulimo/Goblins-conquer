using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.Assertions;

public enum Slot_Type
{
    A, B, C, U,
}

public class Slot_State
{
    public Slot_Type slot_type;
    public int id;
    public Human_Pawn human = null;
    public Goblin_Pawn goblin = null;
    public bool is_empty = true;
}

public struct Swap_Info
{
    public Color color;
    public Sprite sprite;
    public bool flip_x;
}

public class Human_Pawn
{
    public Human_Data data;
}

public class Goblin_Pawn
{
    public Goblin_Data data;
}

public class Battle_Scope
{
    public Battle_State state;

    public Slot_State[] b_group;
    public Slot_State[] a_group;
    public Slot_State[] c_group;


    public Dictionary<Slot_Type, Slot_State[]> slot_type_to_group;

    public Dictionary<(Slot_Type, int), Slot_State> slot_state_look_up;


    public void Init_Scope() {
        state = new Battle_State();

        slot_state_look_up = new Dictionary<(Slot_Type, int), Slot_State>();

        b_group = new Slot_State[Game_Spec.MAX_ENEMY_SLOT];
        for (int i = 1; i <= Game_Spec.MAX_ENEMY_SLOT; i++) {
            var b = new Slot_State() { id = i };
            b_group[i - 1] = b;
            slot_state_look_up.Add((Slot_Type.B, b.id), b);
        }

        a_group = new Slot_State[Game_Spec.MAX_ALLY_SLOT];
        for (int i = 1; i <= Game_Spec.MAX_ALLY_SLOT; i++) {
            var a = new Slot_State() { id = i };
            a_group[i - 1] = a;
            slot_state_look_up.Add((Slot_Type.A, a.id), a);
        }

        c_group = new Slot_State[Game_Spec.MAX_BED_SLOT];
        for (int i = 1; i <= Game_Spec.MAX_BED_SLOT; i++) {
            var c = new Slot_State() { id = i };
            c_group[i - 1] = c;
            slot_state_look_up.Add((Slot_Type.C, c.id), c);
        }

        slot_type_to_group = new Dictionary<Slot_Type, Slot_State[]>() {
            [Slot_Type.A] = a_group,
            [Slot_Type.B] = b_group,
            [Slot_Type.C] = c_group,
        };

    }

}

public static class Battle_Sys
{
    static int[] empty_index_array = new int[Mathf.Max(Game_Spec.MAX_ENEMY_SLOT, Game_Spec.MAX_ALLY_SLOT, Game_Spec.MAX_BED_SLOT)];   
    static System.Random rand = new System.Random();


    public static void Run(Battle_Scope scope, float dt) {
        var state = scope.state;

        state.enemy_spawn_timer.Value -= dt;
        if (state.enemy_spawn_timer.Value <= 0) {
            state.enemy_spawn_timer.Value = Game_Spec.INIT_ENEMY_SPAWN_TIME;
            var index = Get_Empty_Slot(scope.b_group);
            if (index > -1) {
                Spawn_Human_At(scope.b_group, index);
            }
        }
    }

    static int Get_Empty_Slot(Slot_State[] group) {
        System.Array.Clear(empty_index_array, 0, empty_index_array.Length);
        int count = 0;
        for (int i = 0; i < group.Length; i++) {
            if (group[i].is_empty) {
                empty_index_array[count] = i;
                count++;
            }
        }

        if (count > 0) {
            rand.Shuffle(count, empty_index_array);
            return empty_index_array[0];
        }
                
        return -1;
    }

    public static void Swap_Slot_State(Battle_Scope scope, (Slot_Type type, int index)a, (Slot_Type type, int index) b) {
        var state_a = scope.slot_state_look_up[(a.type, a.index)];
        var state_b = scope.slot_state_look_up[(b.type, b.index)];
        Assert.IsNotNull(state_a);
        Assert.IsNotNull(state_b);
        Human_Pawn human_t = state_a.human;
        Goblin_Pawn goblin_t = state_a.goblin;
        bool is_empty_t = state_a.is_empty;
        state_a.human = state_b.human;
        state_a.goblin = state_b.goblin;
        state_a.is_empty = state_b.is_empty;
        state_b.human = human_t;
        state_b.goblin = goblin_t;
        state_b.is_empty = is_empty_t;
    }

    public static void Discard_Pawn_At(Battle_Scope scope, Slot_Type type, int index) {
        var group = scope.slot_type_to_group[type];
        group[index].goblin = null;
        group[index].human = null;
        group[index].is_empty = true;
        //MessageBroker.Default.Publish(new Goblin_Spawned { slot_id = group[index].id, goblin_data = data });
    }

    static void Spawn_Goblin_At(Slot_State[] group, int index) {
        var data = Goblin_Def.Default_Goblin_List[Random.Range(0, 3)];
        var g = new Goblin_Pawn() { data = data };
        group[index].goblin = g;
        group[index].is_empty = false;
        MessageBroker.Default.Publish(new Goblin_Spawned { slot_id = group[index].id, goblin_data = data });
    }

    static void Spawn_Human_At(Slot_State[] group, int index) {
        var data = Human_Def.Default_Human_List[Random.Range(0, 3)];
        var h = new Human_Pawn() { data = data };
        group[index].human = h;
        group[index].is_empty = false;
        MessageBroker.Default.Publish(new Human_Spawned { slot_id = group[index].id, human_data = data });
    }

    static void Spawn_Bed_At(Slot_State[] group, int index) {
        var data = Human_Def.Default_Human_List[Random.Range(0, 3)];
        var h = new Human_Pawn() { data = data };
        group[index].human = h;
        group[index].is_empty = false;
        MessageBroker.Default.Publish(new Bed_Spawned { slot_id = group[index].id, human_data = data });
    }

    public static void Spawn_Goblin_Random(Battle_Scope scope) {
        var index = Get_Empty_Slot(scope.a_group);
        if (index > -1) {
            Spawn_Goblin_At(scope.a_group, index);
        }
    }

    public static void Spawn_Human_Random(Battle_Scope scope) {
        var index = Get_Empty_Slot(scope.b_group);
        if (index > -1) {
            Spawn_Human_At(scope.b_group, index);
        }
    }
    public static void Spawn_Bed_Random(Battle_Scope scope) {
        var index = Get_Empty_Slot(scope.c_group);
        if (index > -1) {
            Spawn_Bed_At(scope.c_group, index);
        }
    }

}
