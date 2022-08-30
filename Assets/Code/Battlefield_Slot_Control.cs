using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using DG.Tweening;


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
    }

    async UniTaskVoid fnHopB() {

        await transform.DOLocalMoveY(5, 1).SetRelative().SetEase(Ease.Linear);
        await UniTask.Delay(300);
        await transform.DOLocalMoveY(-5, 0.66f).SetRelative().SetEase(Ease.OutElastic);

        transform.position = original_location;
    }
    async UniTaskVoid fnHopA() {

        _ = transform.DORotate(new Vector3(0, 0, 360), 1.2f, RotateMode.FastBeyond360);

        await transform.DOLocalMoveY(5, 1).SetRelative().SetEase(Ease.InOutCirc);
        await UniTask.Delay(300);

        await transform.DOScale(new Vector3(2, 1.2f, 1), 0.3f).SetRelative().SetLoops(2, LoopType.Yoyo);

        _ = transform.DORotate(new Vector3(0, 0, 360), 0.7f, RotateMode.FastBeyond360);
        await transform.DOLocalMoveY(-5, 0.66f).SetRelative().SetEase(Ease.OutBounce);
    }

}
