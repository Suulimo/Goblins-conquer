using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UniRx;

public static class Battle_Sys
{
    static int[] empty_index_array = new int[Mathf.Max(Game_Spec.MAX_ENEMY_SLOT, Game_Spec.MAX_ALLY_SLOT, Game_Spec.MAX_BED_SLOT)];
    static System.Random rand = new System.Random();



    public static void Run(Battle_Scope scope, float dt) {
        var state = scope.state;

        if (state.is_auto_spawn) {
            state.enemy_spawn_timer.Value -= dt;
            if (state.enemy_spawn_timer.Value <= 0) {
                //state.enemy_spawn_timer.Value = Game_Spec.INIT_ENEMY_SPAWN_TIME;
                state.enemy_spawn_timer.Value = Data_Manager.data_manager.temp_game_setting.enemy_spawn_cd;
                var index = Get_Empty_Slot(scope.b_group);
                if (index > -1) {
                    Spawn_Human_At((int)scope.state.difficulty, scope.b_group, index);
                    if (scope.state.difficulty < 100.0f) {
                        scope.state.difficulty += Data_Manager.data_manager.temp_game_setting.difficulty_growth_rate;
                    }
                }
            }
        }

        for (int i = 0; i < scope.a_group.Length; i++) {
            var a = scope.a_group[i];
            if (a.goblin != null) {
                var g_state = a.goblin.state;
                if (g_state.is_busy)
                    continue;
                Goblin_Normal_Act(scope, a.goblin, dt, (a.slot_type, a.id));
            }
        }

        for (int i = 0; i < scope.b_group.Length; i++) {
            var b = scope.b_group[i];
            if (b.human != null) {
                var g_state = b.human.state;
                if (g_state.is_busy)
                    continue;
                Human_Normal_Act(scope, b.human, dt, (b.slot_type, b.id));
            }

        }

        for (int i = 0; i < scope.c_group.Length; i++) {
            var c = scope.c_group[i];
            if (c.bed != null && c.goblin != null) {
                var g_state = c.bed.state;
                if (g_state.is_busy)
                    continue;
                Bed_Normal_Act(scope, c.bed, c.goblin, dt, (c.slot_type, c.id));
            }
            if (c.goblin != null) {
                Goblin_Recover_Act(scope, c.goblin, dt, (c.slot_type, c.id));
            }
        }

    }

    static int Get_Nonempty_Slot(Slot_State[] group) {
        System.Array.Clear(empty_index_array, 0, empty_index_array.Length);
        int count = 0;
        for (int i = 0; i < group.Length; i++) {
            if (group[i].is_empty == false) {
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

    public static void Swap_Slot_State(Battle_Scope scope, (Slot_Type type, int index) a, (Slot_Type type, int index) b) {
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

    public static void Set_Slot_Pawn_Busy(Battle_Scope scope, (Slot_Type type, int index) slot, bool to_be_busy) {
        var state = scope.slot_state_look_up[(slot.type, slot.index)];
        if (state.human != null)
            state.human.state.is_busy = to_be_busy;
        if (state.goblin != null)
            state.goblin.state.is_busy = to_be_busy;
    }

    public static bool Ask_Slot_Is_Busy(Battle_Scope scope, (Slot_Type type, int index) slot) {
        var state = scope.slot_state_look_up[(slot.type, slot.index)];
        bool is_busy = false;
        if (state.human != null)
            is_busy |= state.human.state.is_busy;
        if (state.goblin != null)
            is_busy |= state.goblin.state.is_busy;
        return is_busy;
    }


    public static void Discard_Pawn_At(Battle_Scope scope, Slot_Type type, int index) {
        var group = scope.slot_type_to_group[type];
        group[index].goblin = null;
        group[index].human = null;
        group[index].bed = null;
        group[index].is_empty = true;
        //MessageBroker.Default.Publish(new Goblin_Spawned { slot_id = group[index].id, goblin_data = data });
    }

    static void Spawn_Goblin_At(int rank, Slot_State[] group, int index) {
        var data = Goblin_Def.Default_Goblin_List[Random.Range(0, Goblin_Def.Default_Goblin_List.Length)];
        var state = new Goblin_State();
        state.rank = rank;
        state.hp_max = (int)(data.battle.hp_base * Mathf.Pow(data.battle.hp_growth_rate, (rank - 1)));
        state.attack_power = (int)(data.battle.attack_power_base * Mathf.Pow(data.battle.attack_growth_rate, (rank - 1)));
        state.hp.Value = state.hp_max;
        state.attack_cycle.Value = Random.Range(0, data.battle.attack_cd);
        var g = new Goblin_Pawn() { data = data, state = state };

        group[index].goblin = g;
        group[index].is_empty = false;
        MessageBroker.Default.Publish(new Goblin_Spawned { slot_id = group[index].id, goblin_data = data });
    }

    static void Spawn_Human_At(int rank, Slot_State[] group, int index) {
        var setting = Data_Manager.data_manager.temp_game_setting;
        var data = (Random.Range(0, setting.enemy_female_num_weight + setting.enemy_male_num_weight) < setting.enemy_male_num_weight) ? Human_Def.Default_Male_Human_List[0] : Human_Def.Default_Human_List[0];
        var state = new Human_State();
        state.rank = rank;
        state.hp_max = (int)(data.battle.hp_base * Mathf.Pow(data.battle.hp_growth_rate, (rank - 1)));
        state.attack_power= (int)(data.battle.attack_power_base * Mathf.Pow(data.battle.attack_growth_rate, (rank - 1)));
        state.hp.Value = state.hp_max;
        state.attack_cycle.Value = Random.Range(0, data.battle.attack_cd * 0.5f);
        var h = new Human_Pawn() { data = data, state = state };

        group[index].human = h;
        group[index].is_empty = false;
        MessageBroker.Default.Publish(new Human_Spawned { slot_id = group[index].id, human_data = data });
    }

    static void Spawn_Bed_At(int rank, int bed_number, Slot_State[] group, int index) {
        var data = Human_Def.Default_Human_List[bed_number];
        var state = new Human_State();
        state.rank = rank;
        state.attack_cycle.Value = 0/*data.battle.attack_cd*/;

        var h = new Human_Pawn() { data = data, state = state };
        group[index].bed = h;
        MessageBroker.Default.Publish(new Bed_Spawned { slot_id = group[index].id, human_data = data });
    }

    public static void Spawn_Goblin_Random(int rank, Battle_Scope scope) {
        var index = Get_Empty_Slot(scope.a_group);
        if (index > -1) {
            Spawn_Goblin_At(rank, scope.a_group, index);
        }
    }

    public static void Spawn_Human_Random(Battle_Scope scope) {
        var index = Get_Empty_Slot(scope.b_group);
        if (index > -1) {
            Spawn_Human_At((int)scope.state.difficulty + Random.Range(0, 3), scope.b_group, index);
        }
    }

    public static void Spawn_Bed_Random(int rank, int bed_number, Battle_Scope scope) {
        //var index = Get_Empty_Slot(scope.c_group);
        System.Array.Clear(empty_index_array, 0, empty_index_array.Length);
        int count = 0;
        for (int i = 0; i < scope.c_group.Length; i++) {
            if (scope.c_group[i].bed == null) {
                empty_index_array[count] = i;
                count++;
            }
        }

        if (count > 0) {
            rand.Shuffle(count, empty_index_array);
            Spawn_Bed_At(rank, bed_number, scope.c_group, empty_index_array[0]);
        }
    }

    public static void Goblin_Normal_Act(Battle_Scope scope, Goblin_Pawn pawn, float dt, (Slot_Type type, int id) slot_info) {
        var g_state = pawn.state;
        var g_data = pawn.data;
        g_state.attack_cycle.Value = Mathf.Min(g_state.attack_cycle.Value + dt, g_data.battle.attack_cd);
        if (g_state.attack_cycle.Value == g_data.battle.attack_cd) {
            var me = Game_Control.game_control.Hack_Ask_Position(slot_info.type, slot_info.id);
            (Slot_Type scan_type, int scan_id, Vector3 scan_position) = Game_Control.game_control.Hack_Scan_Target(me, LayerMask.GetMask("Takeable_B"));
            if (scan_id != -1) {
                MessageBroker.Default.Publish(new Goblin_Attack_Human { goblin_slot_id = slot_info.id, human_slot_id = scan_id });
                g_state.attack_cycle.Value = 0;
            }
        }
    }

    public static void Goblin_Recover_Act(Battle_Scope scope, Goblin_Pawn pawn, float dt, (Slot_Type type, int id) slot_info) {
        var g_state = pawn.state;
        var g_data = pawn.data;
        g_state.attack_cycle.Value = Mathf.Min(g_state.attack_cycle.Value + dt * g_data.battle.attack_cd, g_data.battle.attack_cd);
        if (g_state.attack_cycle.Value == g_data.battle.attack_cd) {
            g_state.attack_cycle.Value = 0;
            g_state.hp.Value = Mathf.Min(g_state.hp.Value + Data_Manager.data_manager.temp_game_setting.bed_recovery_rate, g_state.hp_max);
        }
    }


    public static void Human_Normal_Act(Battle_Scope scope, Human_Pawn pawn, float dt, (Slot_Type type, int id) slot_info) {
        var g_state = pawn.state;
        var g_data = pawn.data;
        g_state.attack_cycle.Value = Mathf.Min(g_state.attack_cycle.Value + dt, g_data.battle.attack_cd);
        if (g_state.attack_cycle.Value == g_data.battle.attack_cd) {
            var me = Game_Control.game_control.Hack_Ask_Position(slot_info.type, slot_info.id);
            (Slot_Type scan_type, int scan_id, Vector3 scan_position) = Game_Control.game_control.Hack_Scan_Target(me, LayerMask.GetMask("Takeable_A"));
            if (scan_id != -1) {
                MessageBroker.Default.Publish(new Human_Attack_Goblin { human_slot_id = slot_info.id, goblin_slot_id = scan_id });
                g_state.attack_cycle.Value = 0;
            }
        }
    }

    public static void Bed_Normal_Act(Battle_Scope scope, Human_Pawn pawn, Goblin_Pawn goblin_pawn, float dt, (Slot_Type type, int id) slot_info) {
        var b_state = pawn.state;
        var b_data = pawn.data;
        b_state.attack_cycle.Value = Mathf.Min(b_state.attack_cycle.Value + dt * Mathf.Pow(Data_Manager.data_manager.temp_game_setting.bed_spawn_accelerate, (b_state.rank - 1)), b_data.battle.bed_spawn_cd);
        if (b_state.attack_cycle.Value == b_data.battle.bed_spawn_cd) {
            var index = Get_Empty_Slot(scope.a_group);
            if (index > -1) {
                Spawn_Goblin_At(((b_state.rank + goblin_pawn.state.rank) / 2), scope.a_group, index);
                b_state.attack_cycle.Value = 0;
            }
        }
    }

    public static void Human_Attack_Goblin(Battle_Scope scope, Human_Pawn human_pawn, int human_slot_id, Goblin_Pawn goblin_pawn, int goblin_slot_id) {
        goblin_pawn.state.hp.Value -= human_pawn.state.attack_power;

        if (goblin_pawn.state.hp.Value <= 0) {
            // die
            scope.a_group[goblin_slot_id - 1].goblin = null;
            scope.a_group[goblin_slot_id - 1].human = null;
            scope.a_group[goblin_slot_id - 1].is_empty = true;

            Game_Control.game_control.Hack_Pawn_Die(Slot_Type.A, goblin_slot_id);
        }
    }

    public static void Goblin_Attack_Human(Battle_Scope scope, Human_Pawn human_pawn, int human_slot_id, Goblin_Pawn goblin_pawn, int goblin_slot_id) {
        human_pawn.state.hp.Value -= goblin_pawn.state.attack_power;

        if (human_pawn.state.hp.Value <= 0) {
            // die
            scope.b_group[human_slot_id - 1].goblin = null;
            scope.b_group[human_slot_id - 1].human = null;
            scope.b_group[human_slot_id - 1].is_empty = true;

            Game_Control.game_control.Hack_Pawn_Die(Slot_Type.B, human_slot_id);

            if (human_pawn.data.other.femininity > 0)
                Spawn_Bed_Random(human_pawn.state.rank, human_pawn.data.other.beauty, scope);
        }
    }

}
