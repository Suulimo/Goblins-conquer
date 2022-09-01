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

    Dictionary<string, Battlefield_Slot_Control> slot_look_up;
    //Dictionary<Slot_Type, Battlefield_Slot_Control[]> slot_type_to_group;

    void Awake() {
        MessageBroker.Default.Receive<Drag_To>().Subscribe(args => OnDragTo(args)).AddTo(this);
        MessageBroker.Default.Receive<Drag_To_Cancel>().Subscribe(args => OnDragToCancel()).AddTo(this);
        MessageBroker.Default.Receive<Drag_Begin>().Subscribe(args => OnDragBegin(args)).AddTo(this);
        MessageBroker.Default.Receive<Drag_End>().Subscribe(args => OnDragEnd(args)).AddTo(this);
        MessageBroker.Default.Receive<Drag_Cancel>().Subscribe(args => OnDragCancel()).AddTo(this);

        MessageBroker.Default.Receive<Human_Spawned>().Subscribe(args => On_Spawn_Human(args)).AddTo(this);
        MessageBroker.Default.Receive<Goblin_Spawned>().Subscribe(args => On_Spawn_Goblin(args)).AddTo(this);
        MessageBroker.Default.Receive<Bed_Spawned>().Subscribe(args => On_Spawn_Bed(args)).AddTo(this);

        slot_look_up = new Dictionary<string, Battlefield_Slot_Control>();

        foreach (var o in a_group) {
            slot_look_up.Add(o.slot_type.ToString() + o.slot_id, o);
        }
        foreach (var o in b_group) {
            slot_look_up.Add(o.slot_type.ToString() + o.slot_id, o);
        }
        foreach (var o in c_group) {
            slot_look_up.Add(o.slot_type.ToString() + o.slot_id, o);
        }

        //slot_type_to_group = new Dictionary<Slot_Type, Battlefield_Slot_Control[]>() {
        //    [Slot_Type.A] = a_group,
        //    [Slot_Type.B] = b_group,
        //    [Slot_Type.C] = c_group,
        //};
    }

    // Start is called before the first frame update
    void Start() {
        UniTaskAsyncEnumerable.EveryUpdate().Subscribe(_ => {
            My_Update(Time.deltaTime);
        }).AddTo(gameObject);
    }

    void My_Update(float dt) {
        if (is_dragging) {
            var cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursor.z = 0;
            dragged.transform.position = cursor;
        }
    }

    void OnDragTo(Drag_To args) {
        if (is_dragging == false)
            return;

        to_slot_type = args.slot_type;
        to_slot_id = args.slot_id;

        is_drag_ready = true;

        Debug.Log("drag to set " + args.slot_type + args.slot_id);
    }

    void OnDragToCancel() {
        if (is_dragging == false)
            return;
        is_drag_ready = false;
    }

    void OnDragBegin(Drag_Begin args) {
        if (is_dragging)
            return;

        var slot = slot_look_up[args.slot_type.ToString() + args.slot_id];
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
                Debug.Log("discard " + args.slot_type + args.slot_id);
                var discard = slot_look_up[args.slot_type.ToString() + args.slot_id];
                if (discard != null) {
                    SpawnPrefabSystem.Return(discard.Get_Pawn_Object);
                    Battle_Sys.Discard_Pawn_At(Static_Game_Scope.battle_scope, args.slot_type, args.slot_id - 1);
                }

            }
            else {
                // 交換或移動
                Debug.Log("drag end " + args.slot_type + args.slot_id + "<--->" + to_slot_type + to_slot_id);


                var swap_a = slot_look_up[to_slot_type.ToString() + to_slot_id];
                var swap_b = slot_look_up[args.slot_type.ToString() + args.slot_id];

                if (swap_a && swap_b && (swap_a.slot_type == swap_b.slot_type)) {
                    var state_a = battle_scope.slot_state_look_up[(swap_a.slot_type, swap_a.slot_id)];
                    var state_b = battle_scope.slot_state_look_up[(swap_b.slot_type, swap_b.slot_id)];

                    // 交換
                    if (state_a.is_empty == false && state_b.is_empty == false) {
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
        var to_slot = slot_look_up[Slot_Type.A.ToString() + args.slot_id];
        if (to_slot != null) {
            var p = SpawnPrefabSystem.Rent("Pawn", to_slot.transform, false);

            var render = p.gameObj.GetComponent<SpriteRenderer>();
            render.sprite = Data_Manager.data_manager.share_pic.goblin_pic[args.goblin_data.other.sprite];
            render.flipX = true;

            to_slot.Set_Pawn_Object(p.gameObj);
        }

    }
    void On_Spawn_Human(Human_Spawned args) {
        var to_slot = slot_look_up[Slot_Type.B.ToString() + args.slot_id];
        if (to_slot != null) {
            var p = SpawnPrefabSystem.Rent("Pawn", to_slot.transform, false);

            var render = p.gameObj.GetComponent<SpriteRenderer>();
            render.sprite = Data_Manager.data_manager.share_pic.human_pic[args.human_data.other.sprite];
            render.flipX = false;

            to_slot.Set_Pawn_Object(p.gameObj);
        }

    }

    void On_Spawn_Bed(Bed_Spawned args) {
        var to_slot = slot_look_up[Slot_Type.C.ToString() + args.slot_id];
        if (to_slot != null) {
            var p = SpawnPrefabSystem.Rent("Pawn", to_slot.transform, false);

            var render = p.gameObj.GetComponent<SpriteRenderer>();
            render.sprite = Data_Manager.data_manager.share_pic.human_pic[args.human_data.other.sprite];
            render.flipX = false;

            to_slot.Set_Pawn_Object(p.gameObj);
        }
    }

    async UniTaskVoid Tk_Swap_Pawn(Battlefield_Slot_Control slot_a, Battlefield_Slot_Control slot_b) {
        slot_a.Lock_Collider = true;
        slot_b.Lock_Collider = true;

        // state
        Battle_Sys.Swap_Slot_State(battle_scope, (slot_a.slot_type, slot_a.slot_id), (slot_b.slot_type, slot_b.slot_id));
        
        var pawn_a = slot_a.Get_Pawn_Object;
        var pawn_b = slot_b.Get_Pawn_Object;

        _ = pawn_a.transform.DOMove(slot_b.transform.position, 1);
        await pawn_b.transform.DOMove(slot_a.transform.position, 1);

        slot_a.Set_Pawn_Object(pawn_b);
        slot_b.Set_Pawn_Object(pawn_a);

        slot_a.Lock_Collider = false;
        slot_b.Lock_Collider = false;
    }

    async UniTaskVoid Tk_Move_Pawn(Battlefield_Slot_Control slot_a, Battlefield_Slot_Control slot_b) {
        slot_a.Lock_Collider = true;
        slot_b.Lock_Collider = true;

        // state
        Battle_Sys.Swap_Slot_State(battle_scope, (slot_a.slot_type, slot_a.slot_id), (slot_b.slot_type, slot_b.slot_id));
        
        var pawn_a = slot_a.Get_Pawn_Object;
        var pawn_b = slot_b.Get_Pawn_Object;

        //_ = pawn_a.transform.DOMove(pawn_b.transform.position, 1);
        await pawn_b.transform.DOMove(slot_a.transform.position, 1);

        slot_a.Set_Pawn_Object(pawn_b);
        slot_b.Set_Pawn_Object(pawn_a);

        slot_a.Lock_Collider = false;
        slot_b.Lock_Collider = false;
    }

}