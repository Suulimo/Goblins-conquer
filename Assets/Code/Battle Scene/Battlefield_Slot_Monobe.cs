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
    SpriteRenderer pawon_sprite_renderer;

    GameObject slot_display_object;

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
        pawon_sprite_renderer = value?.GetComponent<SpriteRenderer>();
    }
    public GameObject Get_Pawn_Object => pawn_object;

    public void Set_Slot_Display_Object(GameObject value) {
        slot_display_object = value;
    }

    public GameObject Get_Slot_Display_Object => slot_display_object;


    public GCQ.Swap_Info Get_Swap_Info => new GCQ.Swap_Info {
        sprite = pawon_sprite_renderer != null ? pawon_sprite_renderer.sprite : null,
        flip_x = pawon_sprite_renderer != null && pawon_sprite_renderer.flipX,
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
        sprite_renderer.color = Color.white;
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

    // Start is called before the first frame update
    void Start() {

    }

    private void OnMouseOver() {

    }

    private void OnMouseEnter() {
        if (GCQ.Static_Game_Scope.battlefield_main_ref.Use.current_cursor_mode != GCQ.Battlefield_Use.Cursor_Mode.Drag)
            return;

        var c = Color.white * 0.5f + original_color * 0.5f;
        c.a = 1;
        sprite_renderer.color = c;
        MessageBroker.Default.Publish(new Drag_To { slot_id = slot_id, slot_type = slot_type });
    }

    private void OnMouseExit() {
        if (GCQ.Static_Game_Scope.battlefield_main_ref.Use.current_cursor_mode != GCQ.Battlefield_Use.Cursor_Mode.Drag)
            return;
        MessageBroker.Default.Publish(new Drag_To_Cancel { });
        sprite_renderer.color = original_color;
    }

    private void OnMouseDown() {
        if (GCQ.Static_Game_Scope.battlefield_main_ref.Use.current_cursor_mode != GCQ.Battlefield_Use.Cursor_Mode.Drag)
            return;
        if (slot_type == GCQ.Slot_Type.U)
            return;
        MessageBroker.Default.Publish(new Drag_Begin { slot_id = slot_id, slot_type = slot_type });
    }

    private void OnMouseUp() {
        if (GCQ.Static_Game_Scope.battlefield_main_ref.Use.current_cursor_mode != GCQ.Battlefield_Use.Cursor_Mode.Drag)
            return;
        MessageBroker.Default.Publish(new Drag_End { slot_id = slot_id, slot_type = slot_type });
    }

    private void OnMouseUpAsButton() {
        switch (slot_type) {
            case GCQ.Slot_Type.C:
            case GCQ.Slot_Type.B:
            case GCQ.Slot_Type.A:
                MessageBroker.Default.Publish(new Selection_Done { slot_id = slot_id, slot_type = slot_type });

                break;
        }
    }


    async UniTaskVoid fnHopB() {
        my_collider2D.enabled = false;

        await transform.DOLocalMoveY(1.5f, 0.2f).SetRelative().SetEase(Ease.Linear);
        await UniTask.Delay(250);
        await transform.DOLocalMoveY(-1.5f, 0.16f).SetRelative().SetEase(Ease.OutElastic);

        transform.position = original_location;
        my_collider2D.enabled = true;
    }

}
