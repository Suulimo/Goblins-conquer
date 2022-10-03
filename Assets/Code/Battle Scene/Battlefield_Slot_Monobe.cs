using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class Battlefield_Slot_Monobe : MonoBehaviour
{
    public GCQ.Slot_Type slot_type;
    public int2 slot_id;

    public Vector3 original_location;

    SpriteRenderer sprite_renderer;
    Color original_color;
    Collider2D my_collider2D;

    GameObject pawn_object;
    SpriteRenderer pawn_sprite_renderer;
    Pawn_Monobe pawn_monobe;

    Slot_Display_Monobe slot_display_object;

    GCQ.Slot_Data data;

    public void Set_Data(GCQ.Slot_Data value) {
        data = value;
        Assert.IsNotNull(data);
    }

    public GCQ.Slot_Data Data => data;

    public bool Lock_Collider {
        set => my_collider2D.enabled = !value;
    }

    public void Set_Pawn_Object(GameObject value) {
        pawn_object = value;
        pawn_sprite_renderer = value?.GetComponent<SpriteRenderer>();
        pawn_monobe = null;
        value?.TryGetComponent<Pawn_Monobe>(out pawn_monobe);
    }

    public GameObject Get_Pawn_Object => pawn_object;

    public void Set_Slot_Display_Object(Slot_Display_Monobe value) {
        slot_display_object = value;
    }

    public Slot_Display_Monobe Get_Slot_Display_Object => slot_display_object;

    public Pawn_Monobe Get_Pawn_Monobe => pawn_monobe;

    public GCQ.Swap_Info Get_Swap_Info => new GCQ.Swap_Info {
        sprite = pawn_sprite_renderer != null ? pawn_sprite_renderer.sprite : null,
        flip_x = pawn_sprite_renderer != null && pawn_sprite_renderer.flipX,
        color = original_color,
    };

    public void Set_Swap_Info(GCQ.Swap_Info info) {
        original_color = info.color;
        sprite_renderer.color = original_color;
    }

    public void Reset_Color() {
        sprite_renderer.color = original_color;
    }

    public void Set_Mark_Color() {
        var c = Color.white * 0.5f + original_color * 0.5f;
        c.a = 1;
        sprite_renderer.color = c;
    }

    public void Set_Selected_Color() {
        var c = Color.black * 0.5f + original_color * 0.5f;
        c.a = 1;
        sprite_renderer.color = c;
    }

    private void Awake() {
        original_location = transform.position;
        sprite_renderer = GetComponent<SpriteRenderer>();
        original_color = sprite_renderer.color;
        my_collider2D = GetComponent<Collider2D>();
    }

    public void Reset_Original() {
        original_location = transform.position;
        original_color = sprite_renderer.color;
    }
}
