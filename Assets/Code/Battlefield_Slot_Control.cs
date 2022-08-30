using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;


public class Battlefield_Slot_Control : MonoBehaviour
{
    public Slot_Type slot_type;
    public int slot_id;

    public Vector3 original_location;

    SpriteRenderer sprite_renderer;
    Color original_color;

    public Swap_Info Get_Swap_Info => new Swap_Info { color = original_color };

    public void Set_Swap_Info(Swap_Info info) {
        original_color = info.color;
        sprite_renderer.color = original_color;
    }

    private void Awake() {
        original_location = transform.position;
        sprite_renderer = GetComponent<SpriteRenderer>();
        original_color = sprite_renderer.color;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnMouseOver() {
       
    }

    private void OnMouseEnter() {
        //Debug.Log(gameObject.name + "E");
        var c = Color.white * 0.5f + original_color * 0.5f;
        c.a = 1;
        sprite_renderer.color = c;
        MessageBroker.Default.Publish(new DragTo { slot_id = slot_id, slot_type = slot_type });
    }

    private void OnMouseExit() {
        //Debug.Log(gameObject.name + "X");
        MessageBroker.Default.Publish(new DragToCancel{ });

        sprite_renderer.color = original_color;
    }

    private void OnMouseDown() {
        MessageBroker.Default.Publish(new DragBegin { slot_id = slot_id, slot_type = slot_type });
    }

    private void OnMouseUp() {
        Debug.Log(gameObject.name + "X");
        MessageBroker.Default.Publish(new DragEnd { slot_id = slot_id, slot_type = slot_type });
    }

    private void OnMouseUpAsButton() {
        if (slot_type == Slot_Type.C) {

        }
    }
}
