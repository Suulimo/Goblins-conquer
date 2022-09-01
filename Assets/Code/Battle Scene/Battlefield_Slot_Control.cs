using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using static Static_Game_Scope;

public class Battlefield_Slot_Control : MonoBehaviour
{
    public Slot_Type slot_type;
    public int slot_id;

    public Vector3 original_location;

    SpriteRenderer sprite_renderer;
    Color original_color;
    Collider2D my_collider2D;

    GameObject pawn_object;
    SpriteRenderer pawon_sprite_renderer;

    public bool Lock_Collider {
        set => my_collider2D.enabled = !value;
    }

    public void Set_Pawn_Object(GameObject value) {
        pawn_object = value;
        pawon_sprite_renderer = value?.GetComponent<SpriteRenderer>();
    }
    public GameObject Get_Pawn_Object => pawn_object;


    public Swap_Info Get_Swap_Info => new Swap_Info {
        sprite = pawon_sprite_renderer != null ? pawon_sprite_renderer.sprite : null,
        flip_x = pawon_sprite_renderer != null && pawon_sprite_renderer.flipX,
        color = original_color,
    };

    public void Set_Swap_Info(Swap_Info info) {
        original_color = info.color;
        sprite_renderer.color = original_color;
    }

    private void Awake() {
        original_location = transform.position;
        sprite_renderer = GetComponent<SpriteRenderer>();
        original_color = sprite_renderer.color;
        my_collider2D = GetComponent<Collider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnMouseOver() {
       
    }

    private void OnMouseEnter() {
        var c = Color.white * 0.5f + original_color * 0.5f;
        c.a = 1;
        sprite_renderer.color = c;
        MessageBroker.Default.Publish(new Drag_To { slot_id = slot_id, slot_type = slot_type });
    }

    private void OnMouseExit() {
        MessageBroker.Default.Publish(new Drag_To_Cancel{ });
        sprite_renderer.color = original_color;
    }

    private void OnMouseDown() {
        if (slot_type == Slot_Type.U)
            return;
        MessageBroker.Default.Publish(new Drag_Begin { slot_id = slot_id, slot_type = slot_type });
    }

    private void OnMouseUp() {
        Debug.Log(gameObject.name + "X");
        MessageBroker.Default.Publish(new Drag_End { slot_id = slot_id, slot_type = slot_type });
    }

    private void OnMouseUpAsButton() {
        if (slot_type == Slot_Type.C) {
            fnHopC().Forget();
        }
        else if (slot_type == Slot_Type.B) {
            fnHopB().Forget();
        }
        else if (slot_type == Slot_Type.A) {
            fnHopA().Forget();
        }
    }

    async UniTaskVoid fnHopC() {
        my_collider2D.enabled = false;
        Vector3 tall = new Vector3(0, 0, 0);

        while (tall.y < 5) {
            tall.y += 5f * Time.deltaTime;
            transform.position = original_location + tall;
            await UniTask.NextFrame();
        }

        transform.position = original_location + tall;

        await UniTask.Delay(300);

        while (tall.y > 0) {
            tall.y -= 7.5f * Time.deltaTime;
            transform.position = original_location + tall;
            await UniTask.NextFrame();
        }

        transform.position = original_location;
        my_collider2D.enabled = true;
    }

    async UniTaskVoid fnHopB() {
        my_collider2D.enabled = false;

        await transform.DOLocalMoveY(5, 1).SetRelative().SetEase(Ease.Linear);
        await UniTask.Delay(300);
        await transform.DOLocalMoveY(-5, 0.66f).SetRelative().SetEase(Ease.OutElastic);

        transform.position = original_location;
        my_collider2D.enabled = true;
    }
    async UniTaskVoid fnHopA() {
        my_collider2D.enabled = false;

        _ = transform.DORotate(new Vector3(0, 0, 360), 1.2f, RotateMode.FastBeyond360);

        await transform.DOLocalMoveY(5, 1).SetRelative().SetEase(Ease.InOutCirc);
        await UniTask.Delay(300);

        await transform.DOScale(new Vector3(2, 1.2f, 1), 0.3f).SetRelative().SetLoops(2, LoopType.Yoyo);

        _ = transform.DORotate(new Vector3(0, 0, 360), 0.7f, RotateMode.FastBeyond360);
        await transform.DOLocalMoveY(-5, 0.66f).SetRelative().SetEase(Ease.OutBounce);

        transform.position = original_location;
        my_collider2D.enabled = true;
    }

}
