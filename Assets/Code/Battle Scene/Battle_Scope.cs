using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public enum Slot_Type
{
    A, B, C, U,
}

public class Slot_State
{
    public Slot_Type slot_type;
    public int id;
    public Human_Pawn human = null;
    public Human_Pawn bed = null;
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
    public Human_State state;
}

public class Human_State
{
    public bool is_busy;
    public IntReactiveProperty hp = new IntReactiveProperty();
    public FloatReactiveProperty attack_cycle = new FloatReactiveProperty();
    public int hp_max;
    public int attack_power;
    public int rank;
}

public class Goblin_Pawn
{
    public Goblin_Data data;
    public Goblin_State state;
}

public class Goblin_State
{
    public bool is_busy;
    public IntReactiveProperty hp = new IntReactiveProperty();
    public FloatReactiveProperty attack_cycle = new FloatReactiveProperty();
    public int hp_max;
    public int attack_power;
    public int rank;
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
    }

    public void Init_Scope_Slot(int num_a, int num_b, int num_c) {
        slot_state_look_up = new Dictionary<(Slot_Type, int), Slot_State>();

        b_group = new Slot_State[num_b];
        for (int i = 1; i <= num_b; i++) {
            var b = new Slot_State() { id = i, slot_type = Slot_Type.B };
            b_group[i - 1] = b;
            slot_state_look_up.Add((Slot_Type.B, b.id), b);
        }

        a_group = new Slot_State[num_a];
        for (int i = 1; i <= num_a; i++) {
            var a = new Slot_State() { id = i, slot_type = Slot_Type.A };
            a_group[i - 1] = a;
            slot_state_look_up.Add((Slot_Type.A, a.id), a);
        }

        c_group = new Slot_State[num_c];
        for (int i = 1; i <= num_c; i++) {
            var c = new Slot_State() { id = i, slot_type = Slot_Type.C };
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

