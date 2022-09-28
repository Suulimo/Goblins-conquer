using System.Collections.Generic;
using UniRx;
using Unity.Mathematics;
using UnityEngine;

namespace GCQ
{
    public static class Battle_Sys
    {
        [ClearOnReload]
        static int2[] empty_index_array = new int2[Game_Spec.MAX_ROW_LENGTH * Mathf.Max(Game_Spec.MAX_ENEMY_SLOT_ROW, Game_Spec.MAX_ALLY_SLOT_ROW, Game_Spec.MAX_BED_SLOT_ROW)];

        [ClearOnReload]
        static System.Random rand = new System.Random();

        [ExecuteOnReload]
        static void Reload_Sys() {
            empty_index_array = new int2[Game_Spec.MAX_ROW_LENGTH * Mathf.Max(Game_Spec.MAX_ENEMY_SLOT_ROW, Game_Spec.MAX_ALLY_SLOT_ROW, Game_Spec.MAX_BED_SLOT_ROW)];
            rand = new System.Random();
        }


        public static void Run(Battle_Scope scope, float dt) {
            if (scope.data.is_auto_spawn) {
                scope.data.enemy_spawn_timer.Value -= dt;
                if (scope.data.enemy_spawn_timer.Value <= 0) {
                    //state.enemy_spawn_timer.Value = Game_Spec.INIT_ENEMY_SPAWN_TIME;
                    scope.data.enemy_spawn_timer.Value = Data_Manager.data_manager.temp_game_setting.enemy_spawn_cd;
                    var (num, arr) = Get_Empty_Slot(scope.slot_group[Slot_Type.B]);
                    if (num > 0) {
                        Spawn_Human_At(scope, (int)scope.data.difficulty, scope.human_slot_group, arr[0]);
                        if (scope.data.difficulty < 100.0f) {
                            scope.data.difficulty += Data_Manager.data_manager.temp_game_setting.difficulty_growth_rate;
                        }
                    }
                }
            }

            foreach (var (k, a) in scope.goblin_slot_group) {
                if (a.goblin != null) {
                    var g_state = a.goblin.combat;
                    if (g_state.is_busy)
                        continue;
                    Goblin_Normal_Act(a, dt);
                }
            }

            foreach (var (k, b) in scope.human_slot_group) {
                if (b.human != null) {
                    var g_state = b.human.combat;
                    if (g_state.is_busy)
                        continue;
                    Human_Normal_Act(b, dt);
                }
            }

            foreach (var (k, c) in scope.bed_slot_group) {
                if (c.bed != null && c.goblin == null) {
                    c.bed.combat.attack_cycle.Value = 0;
                }

                if (c.bed != null && c.goblin != null) {
                    var g_state = c.bed.combat;
                    if (g_state.is_busy)
                        continue;

                    if (c.prev_goblin != c.goblin) {
                        c.bed.combat.attack_cycle.Value = 0;
                        c.prev_goblin = c.goblin;
                    }

                    Bed_Normal_Act(scope, c.bed, c.goblin, dt);
                }
                if (c.goblin != null) {
                    Goblin_Recover_Act(c, dt);
                }
            }

        }

        static int2 Get_Nonempty_Slot(SortedDictionary<int2, Slot_Data> group) {
            System.Array.Clear(empty_index_array, 0, empty_index_array.Length);
            int count = 0;
            foreach (var kv in group) {
                if (kv.Value.is_empty == false) { 
                    empty_index_array[count] = kv.Key;
                    count++;
                }
            }

            if (count > 0) {
                rand.Shuffle(count, empty_index_array);
                return empty_index_array[0];
            }

            return -1;
        }

        static (int, int2[]) Get_Empty_Slot(SortedDictionary<int2, Slot_Data> group) {
            System.Array.Clear(empty_index_array, 0, empty_index_array.Length);
            int count = 0;

            foreach (var kv in group) {
                if (kv.Value.is_empty && kv.Value.is_busy_spawning_or_dying == false) {
                    empty_index_array[count] = kv.Key;
                    count++;
                }
            }

            if (count > 0) {
                rand.Shuffle(count, empty_index_array);
            }
            return (count, empty_index_array);
        }

        public static void Swap_Slot_Data(Slot_Data a, Slot_Data b) {
            (a.human, b.human) = (b.human, a.human);
            (a.goblin, b.goblin) = (b.goblin, a.goblin);
            (a.is_empty, b.is_empty) = (b.is_empty, a.is_empty);
        }

        public static void Set_Slot_Pawn_Busy(Slot_Data a, bool to_be_busy) {
            if (a.human != null)
                a.human.combat.is_busy = to_be_busy;
            if (a.goblin != null)
                a.goblin.combat.is_busy = to_be_busy;
        }

        public static bool Ask_Slot_Is_Busy(Slot_Data a) {
            bool is_busy = false;
            if (a.human != null)
                is_busy |= a.human.combat.is_busy || a.human.combat.melee_queue.Count > 0;
            if (a.goblin != null)
                is_busy |= a.goblin.combat.is_busy || a.goblin.combat.melee_queue.Count > 0;
            return is_busy;
        }


        public static void Discard_Pawn_At(Battle_Scope scope, Slot_Type type, int2 index) {
            var group = scope.slot_group[type];
            group[index].goblin = null;
            group[index].human = null;
            group[index].bed = null;
            group[index].is_empty = true;
            //MessageBroker.Default.Publish(new Goblin_Spawned { slot_id = group[index].id, goblin_data = data });
        }

        public static void Refresh_Spawn_Queue(Battle_Scope_Data bs_data) {
            bs_data.spawn_queue.Clear();
            var setting = Data_Manager.data_manager.temp_game_setting;
            float female_ratio = 0;
            if (setting.enemy_female_num_weight == 0 && setting.enemy_male_num_weight == 0)
                female_ratio = 1;
            else
                female_ratio = setting.enemy_female_num_weight / (setting.enemy_female_num_weight + setting.enemy_male_num_weight);

            int[] arr = new int[5];
            for (int i = 0; i < 5; i++) {
                arr[i] = (i + 1 <= Mathf.FloorToInt(female_ratio * 5)) ? 1 : 0;
            }

            rand.Shuffle(arr);
            for (int i = 0; i < 5; i++) {
                bs_data.spawn_queue.Add(arr[i]);
            }
        }

        public static int Pop_Next_Spawn(Battle_Scope_Data bs_data) {
            var result = bs_data.spawn_queue[0];
            bs_data.spawn_queue.RemoveAt(0);
            if (bs_data.spawn_queue.Count == 0) {
                Refresh_Spawn_Queue(bs_data);
            }
            return result;
        }

        static void Spawn_Goblin_At(int rank, SortedDictionary<int2, Slot_Data> group, int2 index) {
            var spec = Goblin_Def.Default_Goblin_List[UnityEngine.Random.Range(0, Goblin_Def.Default_Goblin_List.Length)];
            var combat = new Pawn_Combat();
            combat.rank = rank;
            combat.hp_max = (int)(spec.combat.hp_base * Mathf.Pow(spec.combat.hp_growth_rate, (rank - 1)));
            combat.attack_power = (int)(spec.combat.attack_power_base * Mathf.Pow(spec.combat.attack_growth_rate, (rank - 1)));
            combat.hp.Value = combat.hp_max;
            combat.attack_cycle.Value = UnityEngine.Random.Range(0, spec.combat.attack_cd * 0.5f);
            var g = new Goblin_Pawn() { spec = spec, combat = combat };

            group[index].goblin = g;
            group[index].is_empty = false;
            MessageBroker.Default.Publish(new Goblin_Spawned { slot_id = group[index].id, goblin_data = spec });
        }

        static void Spawn_Human_At(Battle_Scope scope, int rank, SortedDictionary<int2, Slot_Data> group, int2 index) {
            var setting = Data_Manager.data_manager.temp_game_setting;
            bool is_male = Pop_Next_Spawn(scope.data) == 0;
            var spec = (is_male) ? Human_Def.Default_Male_Human_List[0] : Human_Def.Default_Human_List[0];
            var combat = new Pawn_Combat();
            combat.rank = rank;
            combat.hp_max = (int)(spec.combat.hp_base * Mathf.Pow(spec.combat.hp_growth_rate, (rank - 1)));
            combat.attack_power = (int)(spec.combat.attack_power_base * Mathf.Pow(spec.combat.attack_growth_rate, (rank - 1)));
            combat.hp.Value = combat.hp_max;
            combat.attack_cycle.Value = UnityEngine.Random.Range(0, spec.combat.attack_cd * 0.5f);
            var h = new Human_Pawn() { spec = spec, combat = combat };

            group[index].human = h;
            group[index].is_empty = false;
            MessageBroker.Default.Publish(new Human_Spawned { slot_id = group[index].id, human_data = spec });
        }

        static void Spawn_Bed_At(int rank, int bed_number, SortedDictionary<int2, Slot_Data> group, int2 index) {
            var spec = Human_Def.Default_Human_List[bed_number];
            var combat = new Pawn_Combat();
            combat.rank = rank;
            combat.attack_cycle.Value = 0/*data.battle.attack_cd*/;

            var h = new Human_Pawn() { spec = spec, combat = combat };
            group[index].bed = h;
            MessageBroker.Default.Publish(new Bed_Spawned { slot_id = group[index].id, human_data = spec });
        }

        public static void Spawn_Goblin_Random(int rank, Battle_Scope scope, int num = 1) {
            var (empty_num, arr) = Get_Empty_Slot(scope.goblin_slot_group);
            var spawn_num = Mathf.Min(num, empty_num);
            for (int i = 0; i < spawn_num; i++) {
                Spawn_Goblin_At(rank, scope.goblin_slot_group, arr[i]);
            }
        }

        public static void Spawn_Human_Random(Battle_Scope scope, int num = 1) {
            var (empty_num, arr) = Get_Empty_Slot(scope.human_slot_group);
            var spawn_num = Mathf.Min(num, empty_num);
            for (int i = 0; i < spawn_num; i++) {
                Spawn_Human_At(scope, (int)scope.data.difficulty + UnityEngine.Random.Range(0, 3), scope.human_slot_group, arr[i]);
            }
        }

        public static void Spawn_Bed_Random(int rank, int bed_number, Battle_Scope scope, int num = 1) {
            //var index = Get_Empty_Slot(scope.c_group);
            System.Array.Clear(empty_index_array, 0, empty_index_array.Length);
            int count = 0;

            foreach (var (key, bed_slot) in scope.bed_slot_group) {
                if (bed_slot.bed == null) {
                    empty_index_array[count] = key;
                    count++;
                }
            }

            if (count > 0) {
                rand.Shuffle(count, empty_index_array);
                var spawn_num = Mathf.Min(num, count);
                for (int i = 0; i < spawn_num; i++) {
                    Spawn_Bed_At(rank, bed_number, scope.bed_slot_group, empty_index_array[i]);
                }
            }
        }

        public static void Goblin_Normal_Act(Slot_Data slot, float dt) {
            var combat = slot.goblin.combat;
            var spec = slot.goblin.spec;
            var battle_use = Static_Game_Scope.battlefield_main_ref.Use;
            combat.attack_cycle.Value = Mathf.Min(combat.attack_cycle.Value + dt, spec.combat.attack_cd);
            if (combat.attack_cycle.Value == spec.combat.attack_cd && combat.melee_queue.Count == 0) {
                var me = battle_use.Get_Slot_Position(slot.slot_type, slot.id);
                (Slot_Type scan_type, int2 scan_id, Vector3 scan_position) = battle_use.Scan_Target(me, LayerMask.GetMask("Takeable_B"));
                if ((scan_id.x, scan_id.y) != (-100, -100)) {
                    var msg = new Goblin_Attack_Human { goblin_slot_id = slot.id, human_slot_id = scan_id };
                    battle_use.On_Goblin_vs_Human(msg);
                    MessageBroker.Default.Publish(msg);
                    combat.attack_cycle.Value = 0;
                }
            }
        }

        public static void Goblin_Recover_Act(Slot_Data slot, float dt) {
            var combat = slot.goblin.combat;
            var spec = slot.goblin.spec;
            combat.attack_cycle.Value = Mathf.Min(combat.attack_cycle.Value + dt * spec.combat.attack_cd, spec.combat.attack_cd);
            if (combat.attack_cycle.Value == spec.combat.attack_cd) {
                combat.attack_cycle.Value = 0;
                combat.hp.Value = Mathf.Min(combat.hp.Value + Data_Manager.data_manager.temp_game_setting.bed_recovery_rate, combat.hp_max);
            }
        }


        public static void Human_Normal_Act(Slot_Data slot, float dt) {
            var combat = slot.human.combat;
            var spec = slot.human.spec;
            var battle_use = Static_Game_Scope.battlefield_main_ref.Use;
            combat.attack_cycle.Value = Mathf.Min(combat.attack_cycle.Value + dt, spec.combat.attack_cd);
            if (combat.attack_cycle.Value == spec.combat.attack_cd && combat.melee_queue.Count == 0) {
                var me = battle_use.Get_Slot_Position(slot.slot_type, slot.id);
                (Slot_Type scan_type, int2 scan_id, Vector3 scan_position) = battle_use.Scan_Target(me, LayerMask.GetMask("Takeable_A"));
                if ((scan_id.x, scan_id.y) != (-100, -100)) {
                    var msg = new Human_Attack_Goblin { human_slot_id = slot.id, goblin_slot_id = scan_id };
                    battle_use.On_Human_vs_Goblin(msg);
                    MessageBroker.Default.Publish(msg);
                    combat.attack_cycle.Value = 0;
                }
            }
        }

        public static void Bed_Normal_Act(Battle_Scope scope, Human_Pawn pawn, Goblin_Pawn goblin_pawn, float dt) {
            var combat = pawn.combat;
            var spec = pawn.spec;
            combat.attack_cycle.Value = Mathf.Min(combat.attack_cycle.Value + dt * Mathf.Pow(Data_Manager.data_manager.temp_game_setting.bed_spawn_accelerate, (combat.rank - 1)), spec.combat.bed_spawn_cd);
            if (combat.attack_cycle.Value == spec.combat.bed_spawn_cd) {
                var (num, arr) = Get_Empty_Slot(scope.goblin_slot_group);
                if (num > 0) {
                    Spawn_Goblin_At(((combat.rank + goblin_pawn.combat.rank) / 2), scope.goblin_slot_group, arr[0]);
                    combat.attack_cycle.Value = 0;
                }
            }
        }

        public static void Human_Attack_Goblin(Battle_Scope scope, Human_Pawn human_pawn, int2 human_slot_id, Goblin_Pawn goblin_pawn, int2 goblin_slot_id) {
            if (goblin_pawn.combat.hp.Value <= 0)
                return;
            
            goblin_pawn.combat.hp.Value -= human_pawn.combat.attack_power;

            if (goblin_pawn.combat.hp.Value <= 0) {
                // die
                scope.goblin_slot_group[goblin_slot_id].goblin = null;
                scope.goblin_slot_group[goblin_slot_id].human = null;
                scope.goblin_slot_group[goblin_slot_id].is_empty = true;

                Static_Game_Scope.battlefield_main_ref.Use.On_Pawn_Die(Slot_Type.A, goblin_slot_id);
            }
        }

        public static void Goblin_Attack_Human(Battle_Scope scope, Human_Pawn human_pawn, int2 human_slot_id, Goblin_Pawn goblin_pawn, int2 goblin_slot_id) {
            if (human_pawn.combat.hp.Value <= 0)
                return;
                
            human_pawn.combat.hp.Value -= goblin_pawn.combat.attack_power;

            if (human_pawn.combat.hp.Value <= 0) {
                // die
                scope.human_slot_group[human_slot_id].goblin = null;
                scope.human_slot_group[human_slot_id].human = null;
                scope.human_slot_group[human_slot_id].is_empty = true;

                Static_Game_Scope.battlefield_main_ref.Use.On_Pawn_Die(Slot_Type.B, human_slot_id);

                if (human_pawn.spec.other.femininity > 0)
                    Spawn_Bed_Random(human_pawn.combat.rank, human_pawn.spec.other.beauty, scope);

                if (human_pawn.spec.combat.item_drop_chance.Length > 0) {
                    var drops = human_pawn.spec.combat.item_drop_chance;
                    float totalWeight = 0;

                    foreach (var lt in drops) {
                        totalWeight += lt.drop_chance;
                    }

                    if (totalWeight < 1)
                        totalWeight = 1;

                    float cumulativeTotal = 0;
                    float randomValue = UnityEngine.Random.Range(0, totalWeight);

                    for (int i = 0; i < drops.Length; i++) {
                        cumulativeTotal += drops[i].drop_chance;
                        if (cumulativeTotal > randomValue) {
                            var drop = drops[i].item;
                            switch (drop) {
                                case Battlefield_Use.Item_Test.Birth_Drug:
                                    scope.data.inventory_birth_drug.Value++;
                                    break;
                                case Battlefield_Use.Item_Test.Rage_Drug:
                                    scope.data.inventory_rage_drug.Value++;
                                    break;
                                case Battlefield_Use.Item_Test.Roast_Pork:
                                    scope.data.inventory_roast_pork_num.Value++;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    }

                }
            }
        }
    }
}
