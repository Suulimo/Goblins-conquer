using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UniRx;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;

// 縮短使用命名空間
using static Static_Game_Scope;

public class Battlefield_Use
{
    Battlefield_Main_Component main_component;

    bool is_dragging;
    bool is_drag_ready;

    Slot_Type to_slot_type;
    int to_slot_id;

    Vector3 out_of_screen = new Vector3(-10000, 0, 0);

    Dictionary<(Slot_Type, int), Battlefield_Slot_Component> slot_look_up;
    //Dictionary<Slot_Type, Battlefield_Slot_Control[]> slot_type_to_group;

    Collider2D[] overlapResults = new Collider2D[25];


    public Battlefield_Use(Battlefield_Main_Component battlefield_main_component) {
        main_component = battlefield_main_component;

        MessageBroker.Default.Receive<Drag_To>().Subscribe(args => OnDragTo(args)).AddTo(main_component);
        MessageBroker.Default.Receive<Drag_To_Cancel>().Subscribe(args => OnDragToCancel()).AddTo(main_component);
        MessageBroker.Default.Receive<Drag_Begin>().Subscribe(args => OnDragBegin(args)).AddTo(main_component);
        MessageBroker.Default.Receive<Drag_End>().Subscribe(args => OnDragEnd(args)).AddTo(main_component);
        MessageBroker.Default.Receive<Drag_Cancel>().Subscribe(args => OnDragCancel()).AddTo(main_component);

        MessageBroker.Default.Receive<Human_Spawned>().Subscribe(args => On_Spawn_Human(args)).AddTo(main_component);
        MessageBroker.Default.Receive<Goblin_Spawned>().Subscribe(args => On_Spawn_Goblin(args)).AddTo(main_component);
        MessageBroker.Default.Receive<Bed_Spawned>().Subscribe(args => On_Spawn_Bed(args)).AddTo(main_component);

        // battle
        MessageBroker.Default.Receive<Goblin_Attack_Human>().Subscribe(args => On_Goblin_vs_Human(args)).AddTo(main_component);
        MessageBroker.Default.Receive<Human_Attack_Goblin>().Subscribe(args => On_Human_vs_Goblin(args)).AddTo(main_component);


        slot_look_up = new Dictionary<(Slot_Type, int), Battlefield_Slot_Component>();

        //foreach (var o in main_component.a_group) {
        //    slot_look_up.Add((o.slot_type, o.slot_id), o);
        //}
        //foreach (var o in main_component.b_group) {
        //    slot_look_up.Add((o.slot_type, o.slot_id), o);
        //}
        //foreach (var o in main_component.c_group) {
        //    slot_look_up.Add((o.slot_type, o.slot_id), o);
        //}

    }

    public void Start_My_Update() {
        UniTaskAsyncEnumerable.EveryUpdate().Subscribe(_ => {
            My_Update(Time.deltaTime);
        }).AddTo(main_component);
    }

    void My_Update(float dt) {
        if (is_dragging) {
            var cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursor.z = 0;
            main_component.dragged.transform.position = cursor;
        }
    }

    void OnDragTo(Drag_To args) {
        if (is_dragging == false)
            return;

        to_slot_type = args.slot_type;
        to_slot_id = args.slot_id;

        is_drag_ready = true;
    }

    void OnDragToCancel() {
        if (is_dragging == false)
            return;
        is_drag_ready = false;
    }

    void OnDragBegin(Drag_Begin args) {
        if (is_dragging)
            return;

        var slot = slot_look_up[(args.slot_type, args.slot_id)];
        if (slot == null)
            return;

        var info = slot.Get_Swap_Info;
        //main_component.dragged.color = slot.Get_Swap_Info.color;
        main_component.dragged.sprite = info.sprite;
        main_component.dragged.flipX = info.flip_x;

        is_dragging = true;
    }

    void OnDragEnd(Drag_End args) {

        main_component.dragged.transform.position = out_of_screen;


        if (is_dragging && is_drag_ready) {

            if (to_slot_type == Slot_Type.U) {
                // 丟棄
                var discard = slot_look_up[(args.slot_type, args.slot_id)];
                if (discard != null) {
                    SpawnPrefabSystem.SafeReturn(discard.Get_Pawn_Object);
                    SpawnPrefabSystem.SafeReturn(discard.Get_Slot_Display_Object);
                    Battle_Sys.Discard_Pawn_At(Static_Game_Scope.battle_scope, args.slot_type, args.slot_id - 1);
                }

            }
            else {
                // 交換或移動
                var swap_a = slot_look_up[(to_slot_type, to_slot_id)];
                var swap_b = slot_look_up[(args.slot_type, args.slot_id)];

                if (swap_a && swap_b && (swap_a.slot_type == swap_b.slot_type)) {
                    var state_a = battle_scope.slot_state_look_up[(swap_a.slot_type, swap_a.slot_id)];
                    var state_b = battle_scope.slot_state_look_up[(swap_b.slot_type, swap_b.slot_id)];

                    bool busy_a = Battle_Sys.Ask_Slot_Is_Busy(battle_scope, (swap_a.slot_type, swap_a.slot_id));
                    bool busy_b = Battle_Sys.Ask_Slot_Is_Busy(battle_scope, (swap_b.slot_type, swap_b.slot_id));

                    if (to_slot_type == Slot_Type.C) {
                        if (!state_a.is_empty && !state_b.is_empty && !busy_a && !busy_b && state_a.bed != null && state_b.bed != null) {
                            var c = swap_a.Get_Swap_Info;
                            swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                            swap_b.Set_Swap_Info(c);
                            _ = Tk_Swap_Pawn(swap_a, swap_b);
                        }
                        else if (state_a.is_empty && state_b.is_empty == false && state_a.bed != null) {
                            var c = swap_a.Get_Swap_Info;
                            swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                            swap_b.Set_Swap_Info(c);
                            _ = Tk_Move_Pawn(swap_a, swap_b);
                        }
                    }
                    else {
                        // 交換
                        if (!state_a.is_empty && !state_b.is_empty && !busy_a && !busy_b) {
                            var c = swap_a.Get_Swap_Info;
                            swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                            swap_b.Set_Swap_Info(c);

                            _ = Tk_Swap_Pawn(swap_a, swap_b);
                        }
                        // 移動
                        else if (state_a.is_empty && state_b.is_empty == false) {
                            var c = swap_a.Get_Swap_Info;
                            swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                            swap_b.Set_Swap_Info(c);
                            _ = Tk_Move_Pawn(swap_a, swap_b);
                        }
                    }
                }

                // siu息
                if (swap_a && swap_b && (to_slot_type, args.slot_type) == (Slot_Type.C, Slot_Type.A)) {
                    var state_a = battle_scope.slot_state_look_up[(swap_a.slot_type, swap_a.slot_id)];
                    var state_b = battle_scope.slot_state_look_up[(swap_b.slot_type, swap_b.slot_id)];

                    bool busy_a = Battle_Sys.Ask_Slot_Is_Busy(battle_scope, (swap_a.slot_type, swap_a.slot_id));
                    bool busy_b = Battle_Sys.Ask_Slot_Is_Busy(battle_scope, (swap_b.slot_type, swap_b.slot_id));

                    if (state_a.is_empty && state_b.is_empty == false && state_a.bed != null) {
                        _ = Tk_Goblin_Goto_Bed(swap_a, swap_b);
                    }
                    else if (!state_a.is_empty && !state_b.is_empty && !busy_a && !busy_b) {
                        var c = swap_a.Get_Swap_Info;
                        swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                        swap_b.Set_Swap_Info(c);

                        _ = Tk_Swap_Pawn(swap_a, swap_b);
                    }

                }
                else if (swap_a && swap_b && (to_slot_type, args.slot_type) == (Slot_Type.A, Slot_Type.C)) {
                    var state_a = battle_scope.slot_state_look_up[(swap_a.slot_type, swap_a.slot_id)];
                    var state_b = battle_scope.slot_state_look_up[(swap_b.slot_type, swap_b.slot_id)];

                    bool busy_a = Battle_Sys.Ask_Slot_Is_Busy(battle_scope, (swap_a.slot_type, swap_a.slot_id));
                    bool busy_b = Battle_Sys.Ask_Slot_Is_Busy(battle_scope, (swap_b.slot_type, swap_b.slot_id));
                    
                    if (state_a.is_empty && state_b.is_empty == false) {
                        _ = Tk_Goblin_Goto_Bed(swap_a, swap_b);
                    }
                    else if (!state_a.is_empty && !state_b.is_empty && !busy_a && !busy_b) {
                        var c = swap_a.Get_Swap_Info;
                        swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                        swap_b.Set_Swap_Info(c);

                        _ = Tk_Swap_Pawn(swap_a, swap_b);
                    }

                }
            }
        }

        is_dragging = false;
        is_drag_ready = false;
    }
    void OnDragCancel() {
        main_component.dragged.transform.position = out_of_screen;

        is_dragging = false;
        is_drag_ready = false;
    }

    void On_Spawn_Goblin(Goblin_Spawned args) {
        var to_slot = slot_look_up[(Slot_Type.A, args.slot_id)];
        if (to_slot != null) {
            var p = SpawnPrefabSystem.Rent("Pawn", to_slot.transform, false);

            var render = p.gameObj.GetComponent<SpriteRenderer>();
            render.sprite = Data_Manager.data_manager.share_pic.goblin_pic[args.goblin_data.other.sprite];
            render.flipX = true;

            var state_goblin = battle_scope.slot_state_look_up[(Slot_Type.A, args.slot_id)];
            var pawn_control = p.gameObj.GetComponent<Pawn_Component>();
            if (pawn_control != null) {
                pawn_control.Set_Goblin_Pawn(state_goblin.goblin);
            }

            to_slot.Set_Pawn_Object(p.gameObj);
        }

    }
    void On_Spawn_Human(Human_Spawned args) {
        var to_slot = slot_look_up[(Slot_Type.B, args.slot_id)];
        if (to_slot != null) {
            var p = SpawnPrefabSystem.Rent("Pawn", to_slot.transform, false);

            var render = p.gameObj.GetComponent<SpriteRenderer>();
            render.sprite = Data_Manager.data_manager.share_pic.human_pic[args.human_data.other.sprite];
            render.flipX = false;

            var state_human = battle_scope.slot_state_look_up[(Slot_Type.B, args.slot_id)];
            var pawn_control = p.gameObj.GetComponent<Pawn_Component>();
            if (pawn_control != null) {
                pawn_control.Set_Human_Pawn(state_human.human);
            }

            to_slot.Set_Pawn_Object(p.gameObj);
        }

    }

    void On_Spawn_Bed(Bed_Spawned args) {
        var to_slot = slot_look_up[(Slot_Type.C, args.slot_id)];
        if (to_slot != null) {
            var p = SpawnPrefabSystem.Rent("Temp Slot Display", to_slot.transform, false);

            var render = p.gameObj.GetComponent<SpriteRenderer>();
            render.sprite = Data_Manager.data_manager.share_pic.bed_pic[args.human_data.other.sprite_bed];
            render.flipX = false;

            var state_bed = battle_scope.slot_state_look_up[(Slot_Type.C, args.slot_id)];
            var slot_display_component = p.gameObj.GetComponent<Slot_Display_Component>();
            if (slot_display_component != null) {
                slot_display_component.Set_Bed_Pawn(state_bed.bed);
            }

            to_slot.Set_Slot_Display_Object(p.gameObj);
        }
    }

    void On_Goblin_vs_Human(Goblin_Attack_Human args) {
        var slot_goblin = slot_look_up[(Slot_Type.A, args.goblin_slot_id)];
        var slot_human = slot_look_up[(Slot_Type.B, args.human_slot_id)];

        var state_goblin = battle_scope.slot_state_look_up[(Slot_Type.A, args.goblin_slot_id)];
        var state_human = battle_scope.slot_state_look_up[(Slot_Type.B, args.human_slot_id)];

        if (state_goblin.is_empty || state_human.is_empty)
            return;

        Tk_Goblin_attack_Human(slot_human, slot_goblin, state_human.human, state_goblin.goblin).Forget();
    }

    void On_Human_vs_Goblin(Human_Attack_Goblin args) {
        var slot_goblin = slot_look_up[(Slot_Type.A, args.goblin_slot_id)];
        var slot_human = slot_look_up[(Slot_Type.B, args.human_slot_id)];

        var state_goblin = battle_scope.slot_state_look_up[(Slot_Type.A, args.goblin_slot_id)];
        var state_human = battle_scope.slot_state_look_up[(Slot_Type.B, args.human_slot_id)];

        if (state_goblin.is_empty || state_human.is_empty || state_goblin.goblin == null || state_human.human == null)
            return;

        Tk_Human_attack_Goblin(slot_human, slot_goblin, state_human.human, state_goblin.goblin).Forget();
    }

    async UniTaskVoid Tk_Swap_Pawn(Battlefield_Slot_Component slot_a, Battlefield_Slot_Component slot_b) {
        slot_a.Lock_Collider = true;
        slot_b.Lock_Collider = true;

        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_a.slot_type, slot_a.slot_id), true);
        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_b.slot_type, slot_b.slot_id), true);

        // state
        Battle_Sys.Swap_Slot_State(battle_scope, (slot_a.slot_type, slot_a.slot_id), (slot_b.slot_type, slot_b.slot_id));

        var pawn_a = slot_a.Get_Pawn_Object;
        var pawn_b = slot_b.Get_Pawn_Object;

        _ = pawn_a.transform.DOMove(slot_b.transform.position, 1);
        await pawn_b.transform.DOMove(slot_a.transform.position, 1);

        slot_a.Set_Pawn_Object(pawn_b);
        slot_b.Set_Pawn_Object(pawn_a);

        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_a.slot_type, slot_a.slot_id), false);
        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_b.slot_type, slot_b.slot_id), false);

        slot_a.Lock_Collider = false;
        slot_b.Lock_Collider = false;
    }

    async UniTaskVoid Tk_Goblin_Goto_Bed(Battlefield_Slot_Component slot_a, Battlefield_Slot_Component slot_b) {
        slot_a.Lock_Collider = true;
        slot_b.Lock_Collider = true;

        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_b.slot_type, slot_b.slot_id), true);

        // state
        Battle_Sys.Swap_Slot_State(battle_scope, (slot_a.slot_type, slot_a.slot_id), (slot_b.slot_type, slot_b.slot_id));

        var pawn_a = slot_a.Get_Pawn_Object;
        var pawn_b = slot_b.Get_Pawn_Object;

        //_ = pawn_a.transform.DOMove(pawn_b.transform.position, 1);
        await pawn_b.transform.DOMove(slot_a.transform.position, 1);

        slot_a.Set_Pawn_Object(pawn_b);
        slot_b.Set_Pawn_Object(pawn_a);

        // b-->a  a變b 寫得不清楚 容易搞錯
        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_a.slot_type, slot_a.slot_id), false);

        slot_a.Lock_Collider = false;
        slot_b.Lock_Collider = false;
    }

    async UniTaskVoid Tk_Move_Pawn(Battlefield_Slot_Component slot_a, Battlefield_Slot_Component slot_b) {
        slot_a.Lock_Collider = true;
        slot_b.Lock_Collider = true;

        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_b.slot_type, slot_b.slot_id), true);

        // state
        Battle_Sys.Swap_Slot_State(battle_scope, (slot_a.slot_type, slot_a.slot_id), (slot_b.slot_type, slot_b.slot_id));

        var pawn_a = slot_a.Get_Pawn_Object;
        var pawn_b = slot_b.Get_Pawn_Object;

        //_ = pawn_a.transform.DOMove(pawn_b.transform.position, 1);
        await pawn_b.transform.DOMove(slot_a.transform.position, 1);

        slot_a.Set_Pawn_Object(pawn_b);
        slot_b.Set_Pawn_Object(pawn_a);

        // b-->a  a變b 寫得不清楚 容易搞錯
        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_a.slot_type, slot_a.slot_id), false);

        slot_a.Lock_Collider = false;
        slot_b.Lock_Collider = false;
    }


    async UniTaskVoid Tk_Goblin_attack_Human(Battlefield_Slot_Component slot_human, Battlefield_Slot_Component slot_goblin, Human_Pawn human_pawn, Goblin_Pawn goblin_pawn) {
        slot_human.Lock_Collider = true;
        slot_goblin.Lock_Collider = true;

        //Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_human.slot_type, slot_human.slot_id), true);
        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_goblin.slot_type, slot_goblin.slot_id), true);

        var pawn_object = slot_goblin.Get_Pawn_Object;

        if (slot_human != null) {
            await pawn_object.transform.DOMove(slot_human.transform.position, 0.3f);
            // state
            Battle_Sys.Goblin_Attack_Human(battle_scope, human_pawn, slot_human.slot_id, goblin_pawn, slot_goblin.slot_id);
            await pawn_object.transform.DOMove(slot_goblin.transform.position, 0.3f);
        }
        //Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_human.slot_type, slot_human.slot_id), false);
        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_goblin.slot_type, slot_goblin.slot_id), false);

        slot_human.Lock_Collider = false;
        slot_goblin.Lock_Collider = false;
    }

    async UniTaskVoid Tk_Human_attack_Goblin(Battlefield_Slot_Component slot_human, Battlefield_Slot_Component slot_goblin, Human_Pawn human_pawn, Goblin_Pawn goblin_pawn) {
        slot_human.Lock_Collider = true;
        slot_goblin.Lock_Collider = true;

        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_human.slot_type, slot_human.slot_id), true);
        //Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_goblin.slot_type, slot_goblin.slot_id), true);

        var pawn_object = slot_human.Get_Pawn_Object;

        if (slot_goblin != null) {
            await pawn_object.transform.DOMove(slot_goblin.transform.position, 0.3f);
            // state
            Battle_Sys.Human_Attack_Goblin(battle_scope, human_pawn, slot_human.slot_id, goblin_pawn, slot_goblin.slot_id);
            await pawn_object.transform.DOMove(slot_human.transform.position, 0.3f);
        }
        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_human.slot_type, slot_human.slot_id), false);
        //Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_goblin.slot_type, slot_goblin.slot_id), false);

        slot_human.Lock_Collider = false;
        slot_goblin.Lock_Collider = false;
    }

    public Vector3 Get_Slot_Position(Slot_Type type, int id) {
        var slot = slot_look_up[(type, id)];
        if (slot != null) {
            return slot.transform.position;
        }
        return out_of_screen;
    }

    public (Slot_Type type, int id, Vector3 pos) Get_Slot_Info(GameObject unknown_slot) {
        var slot = unknown_slot.GetComponent<Battlefield_Slot_Component>();
        if (slot != null)
            return (slot.slot_type, slot.slot_id, slot.transform.position);

        return (Slot_Type.U, -1, Vector3.zero);
    }

    public (Slot_Type, int, Vector3) Scan_Target(Vector3 center, int layer) {
        var start = center;
        var num2 = Physics2D.OverlapCircleNonAlloc(start, 40, overlapResults, layer);

        System.Array.Sort(overlapResults, 0, num2, new Point_Distance_Comparer(start));

        if (num2 == 0)
            return (Slot_Type.U, -1, Vector3.zero);

        for (int i = 0; i < num2; i++) {
            var minHit = overlapResults[i];
            var objRef = minHit.gameObject; // 用於判定目標有沒有被其他原因刪除

            var slot_control = objRef.GetComponent<Battlefield_Slot_Component>();
            if (slot_control == null)
                continue;

            var state = battle_scope.slot_state_look_up[(slot_control.slot_type, slot_control.slot_id)];
            if (state.is_empty)
                continue;

            return (slot_control.slot_type, slot_control.slot_id, slot_control.transform.position);
        }
        return (Slot_Type.U, -1, Vector3.zero);
    }

    public void On_Pawn_Die(Slot_Type type, int id) {
        var discard = slot_look_up[(type, id)];
        if (discard != null) {
            SpawnPrefabSystem.SafeReturn(discard.Get_Pawn_Object);
        }
    }


    public (int, int, int) MakeMap(Tilemap tilemap, GameObject slot_clone) {
        // scan
        (BoundsInt bound, int count, Slot_Type type, Color color, int layer)[] loop_setting = {
            (new BoundsInt(-3, -4, 0, 4, 9, 0), 0, Slot_Type.A, new Color32(0, 0, 255, 100), LayerMask.NameToLayer("Takeable_A")),
            (new BoundsInt(3, -4, 0, 2, 9, 0), 0, Slot_Type.B, new Color32(255, 0, 255, 100), LayerMask.NameToLayer("Takeable_B")),
            (new BoundsInt(-6, -4, 0, 2, 9, 0), 0, Slot_Type.C, new Color32(0, 255, 0, 100), LayerMask.NameToLayer("Takeable_C")),
        };

        for (int s = 0; s < loop_setting.Length; s++) {
            ref var setting = ref loop_setting[s];
            for (int i = setting.bound.xMax; i >= setting.bound.xMin; i--) {
                for (int j = setting.bound.yMin; j <= setting.bound.yMax; j++) {
                    var tl = tilemap.GetTile(new Vector3Int(i, j, 0));
                    if (tl != null) {
                        int index_from = setting.count + 1;

                        var new_slot = GameObject.Instantiate(slot_clone);
                        new_slot.GetComponent<SpriteRenderer>().color = setting.color;
                        new_slot.transform.position = tilemap.GetCellCenterWorld(new Vector3Int(i, j, 0));
                        new_slot.layer = setting.layer;
                        new_slot.name = $"slot {setting.type}{index_from}";

                        var slot_component = new_slot.GetComponent<Battlefield_Slot_Component>();
                        slot_component.slot_type = setting.type;
                        slot_component.slot_id = index_from;
                        slot_component.Reset_Original();

                        slot_look_up.Add((slot_component.slot_type, slot_component.slot_id), slot_component);

                        setting.count++;
                    }
                }
            }
        }

        return (loop_setting[0].count, loop_setting[1].count, loop_setting[2].count);
    }

}
