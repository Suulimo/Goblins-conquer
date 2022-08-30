using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

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

    void Awake() {
        MessageBroker.Default.Receive<DragTo>().Subscribe(args => OnDragTo(args)).AddTo(this);
        MessageBroker.Default.Receive<DragToCancel>().Subscribe(args => OnDragToCancel()).AddTo(this);
        MessageBroker.Default.Receive<DragBegin>().Subscribe(args => OnDragBegin(args)).AddTo(this);
        MessageBroker.Default.Receive<DragEnd>().Subscribe(args => OnDragEnd(args)).AddTo(this);
        MessageBroker.Default.Receive<DragCancel>().Subscribe(args => OnDragCancel()).AddTo(this);

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

    void OnDragTo(DragTo args) {
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

    void OnDragBegin(DragBegin args) {
        if (is_dragging)
            return;

        var slot = slot_look_up[args.slot_type.ToString() + args.slot_id];
        if (slot == null)
            return;

        dragged.color = slot.Get_Swap_Info.color;

        is_dragging = true;
    }

    void OnDragEnd(DragEnd args) {

        dragged.transform.position = out_of_screen;

        if (is_dragging && is_drag_ready) {
            Debug.Log("drag end " + args.slot_type + args.slot_id + "<--->" + to_slot_type + to_slot_id);

            var swap_a = slot_look_up[to_slot_type.ToString() + to_slot_id];
            var swap_b = slot_look_up[args.slot_type.ToString() + args.slot_id];

            if (swap_a && swap_b) {
                var c = swap_a.Get_Swap_Info;
                swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                swap_b.Set_Swap_Info(c);
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
}