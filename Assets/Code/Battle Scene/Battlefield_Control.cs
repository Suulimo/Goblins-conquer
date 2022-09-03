using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using static Static_Game_Scope;

public class Battlefield_Control : MonoBehaviour
{
    public Battlefield_Slot_Control[] a_group;
    public Battlefield_Slot_Control[] b_group;
    public Battlefield_Slot_Control[] c_group;

    public SpriteRenderer dragged;

    bool is_dragging;
    bool is_drag_ready;

    Slot_Type to_slot_type;
    int to_slot_id;

    Vector3 out_of_screen = new Vector3(-10000, 0, 0);

    Dictionary<(Slot_Type, int), Battlefield_Slot_Control> slot_look_up;
    //Dictionary<Slot_Type, Battlefield_Slot_Control[]> slot_type_to_group;

    Collider2D[] overlapResults = new Collider2D[25];


    void Awake() {
        MessageBroker.Default.Receive<Drag_To>().Subscribe(args => OnDragTo(args)).AddTo(this);
        MessageBroker.Default.Receive<Drag_To_Cancel>().Subscribe(args => OnDragToCancel()).AddTo(this);
        MessageBroker.Default.Receive<Drag_Begin>().Subscribe(args => OnDragBegin(args)).AddTo(this);
        MessageBroker.Default.Receive<Drag_End>().Subscribe(args => OnDragEnd(args)).AddTo(this);
        MessageBroker.Default.Receive<Drag_Cancel>().Subscribe(args => OnDragCancel()).AddTo(this);

        MessageBroker.Default.Receive<Human_Spawned>().Subscribe(args => On_Spawn_Human(args)).AddTo(this);
        MessageBroker.Default.Receive<Goblin_Spawned>().Subscribe(args => On_Spawn_Goblin(args)).AddTo(this);
        MessageBroker.Default.Receive<Bed_Spawned>().Subscribe(args => On_Spawn_Bed(args)).AddTo(this);

        // battle
        MessageBroker.Default.Receive<Goblin_Attack_Human>().Subscribe(args => On_Goblin_vs_Human(args)).AddTo(this);
        MessageBroker.Default.Receive<Human_Attack_Goblin>().Subscribe(args => On_Human_vs_Goblin(args)).AddTo(this);


        slot_look_up = new Dictionary<(Slot_Type, int), Battlefield_Slot_Control>();

        foreach (var o in a_group) {
            slot_look_up.Add((o.slot_type, o.slot_id), o);
        }
        foreach (var o in b_group) {
            slot_look_up.Add((o.slot_type, o.slot_id), o);
        }
        foreach (var o in c_group) {
            slot_look_up.Add((o.slot_type, o.slot_id), o);
        }

        //slot_type_to_group = new Dictionary<Slot_Type, Battlefield_Slot_Control[]>() {
        //    [Slot_Type.A] = a_group,
        //    [Slot_Type.B] = b_group,
        //    [Slot_Type.C] = c_group,
        //};
    }

    // Start is called before the first frame update
    async UniTaskVoid Start() {
        UniTaskAsyncEnumerable.EveryUpdate().Subscribe(_ => {
            My_Update(Time.deltaTime);
        }).AddTo(gameObject);

        await UniTask.WaitUntil(() => Game_Control.game_control != null);

        Game_Control.game_control.Hack_Ask_Position = Get_Slot_Position;
        Game_Control.game_control.Hack_Ask_Slot = Get_Slot_Info;
        Game_Control.game_control.Hack_Scan_Target = Scan_Target;
        Game_Control.game_control.Hack_Pawn_Die = On_Pawn_Die;
    }

    void OnDestroy() {
        Game_Control.game_control.Hack_Ask_Position = null;// Get_Slot_Position;
        Game_Control.game_control.Hack_Ask_Slot = null;// Get_Slot_Info;
        Game_Control.game_control.Hack_Scan_Target = null;// Scan_Target;
        Game_Control.game_control.Hack_Pawn_Die = null;
    }

    void My_Update(float dt) {
        if (is_dragging) {
            var cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursor.z = 0;
            dragged.transform.position = cursor;
        }
    }

    Vector3 Get_Slot_Position(Slot_Type type, int id) {
        var slot = slot_look_up[(type, id)];
        if (slot != null) {
            return slot.transform.position;
        }
        return out_of_screen;
    }

    (Slot_Type type, int id, Vector3 pos) Get_Slot_Info(GameObject unknown_slot) {
        var slot = unknown_slot.GetComponent<Battlefield_Slot_Control>();
        if (slot != null)
            return (slot.slot_type, slot.slot_id, slot.transform.position);

        return (Slot_Type.U, -1, Vector3.zero);
    }

    (Slot_Type, int, Vector3) Scan_Target(Vector3 center, int layer) {
        var start = center;
        var num2 = Physics2D.OverlapCircleNonAlloc(start, 60, overlapResults, layer);

        System.Array.Sort(overlapResults, 0, num2, new Point_Distance_Comparer(start));

        if (num2 == 0)
            return (Slot_Type.U, -1, Vector3.zero);

        for (int i = 0; i < num2; i++) {
            var minHit = overlapResults[i];
            var objRef = minHit.gameObject; // 用於判定目標有沒有被其他原因刪除

            var slot_control = objRef.GetComponent<Battlefield_Slot_Control>();
            if (slot_control == null)
                continue;

            var state = battle_scope.slot_state_look_up[(slot_control.slot_type, slot_control.slot_id)];
            if (state.is_empty)
                continue;

            return (slot_control.slot_type, slot_control.slot_id, slot_control.transform.position);
        }
        return (Slot_Type.U, -1, Vector3.zero);
    }

    void On_Pawn_Die(Slot_Type type, int id) {
        var discard = slot_look_up[(type, id)];
        if (discard != null) {
            SpawnPrefabSystem.Return(discard.Get_Pawn_Object);
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
        //dragged.color = slot.Get_Swap_Info.color;
        dragged.sprite = info.sprite;
        dragged.flipX = info.flip_x;

        is_dragging = true;
    }

    void OnDragEnd(Drag_End args) {

        dragged.transform.position = out_of_screen;


        if (is_dragging && is_drag_ready) {

            if (to_slot_type == Slot_Type.U) {
                // 丟棄
                var discard = slot_look_up[(args.slot_type, args.slot_id)];
                if (discard != null) {
                    SpawnPrefabSystem.Return(discard.Get_Pawn_Object);
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
        }

        is_dragging = false;
        is_drag_ready = false;
    }
    void OnDragCancel() {
        dragged.transform.position = out_of_screen;

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
            var pawn_control = p.gameObj.GetComponent<Pawn_Control>();
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

            var state_human= battle_scope.slot_state_look_up[(Slot_Type.B, args.slot_id)];
            var pawn_control = p.gameObj.GetComponent<Pawn_Control>();
            if (pawn_control != null) {
                pawn_control.Set_Human_Pawn(state_human.human);
            }

            to_slot.Set_Pawn_Object(p.gameObj);
        }

    }

    void On_Spawn_Bed(Bed_Spawned args) {
        var to_slot = slot_look_up[(Slot_Type.C, args.slot_id)];
        if (to_slot != null) {
            var p = SpawnPrefabSystem.Rent("Pawn", to_slot.transform, false);

            var render = p.gameObj.GetComponent<SpriteRenderer>();
            render.sprite = Data_Manager.data_manager.share_pic.human_pic[args.human_data.other.sprite];
            render.flipX = false;

            to_slot.Set_Pawn_Object(p.gameObj);
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

    async UniTaskVoid Tk_Swap_Pawn(Battlefield_Slot_Control slot_a, Battlefield_Slot_Control slot_b) {
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

    async UniTaskVoid Tk_Move_Pawn(Battlefield_Slot_Control slot_a, Battlefield_Slot_Control slot_b) {
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

    async UniTaskVoid Tk_Goblin_attack_Human(Battlefield_Slot_Control slot_human, Battlefield_Slot_Control slot_goblin, Human_Pawn human_pawn, Goblin_Pawn goblin_pawn) {
        slot_human.Lock_Collider = true;
        slot_goblin.Lock_Collider = true;

        //Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_human.slot_type, slot_human.slot_id), true);
        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_goblin.slot_type, slot_goblin.slot_id), true);

        var pawn_object = slot_goblin.Get_Pawn_Object;

        await pawn_object.transform.DOMove(slot_human.transform.position, 0.3f);
        // state
        Battle_Sys.Goblin_Attack_Human(battle_scope, human_pawn, slot_human.slot_id, goblin_pawn, slot_goblin.slot_id);
        await pawn_object.transform.DOMove(slot_goblin.transform.position, 0.3f);

        //Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_human.slot_type, slot_human.slot_id), false);
        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_goblin.slot_type, slot_goblin.slot_id), false);

        slot_human.Lock_Collider = false;
        slot_goblin.Lock_Collider = false;
    }

    async UniTaskVoid Tk_Human_attack_Goblin(Battlefield_Slot_Control slot_human, Battlefield_Slot_Control slot_goblin, Human_Pawn human_pawn, Goblin_Pawn goblin_pawn) {
        slot_human.Lock_Collider = true;
        slot_goblin.Lock_Collider = true;

        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_human.slot_type, slot_human.slot_id), true);
        //Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_goblin.slot_type, slot_goblin.slot_id), true);
        
        var pawn_object = slot_human.Get_Pawn_Object;

        await pawn_object.transform.DOMove(slot_goblin.transform.position, 0.3f);
        // state
        Battle_Sys.Human_Attack_Goblin(battle_scope, human_pawn, slot_human.slot_id, goblin_pawn, slot_goblin.slot_id);
        await pawn_object.transform.DOMove(slot_human.transform.position, 0.3f);

        Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_human.slot_type, slot_human.slot_id), false);
        //Battle_Sys.Set_Slot_Pawn_Busy(battle_scope, (slot_goblin.slot_type, slot_goblin.slot_id), false);

        slot_human.Lock_Collider = false;
        slot_goblin.Lock_Collider = false;
    }

}