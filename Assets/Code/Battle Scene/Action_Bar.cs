using UnityEngine;
using UniRx;

public class Action_Bar : MonoBehaviour
{
    public GameObject statusBar;

    SerialDisposable sedi = new SerialDisposable();
    Vector3 originScale;

    void Awake() {
        originScale = transform.localScale;
    }

    public void Init(ReactiveProperty<float> progress, float fullValue) {
        sedi.Disposable = progress.Skip(1).Subscribe(_energy => {
            var ratio = (float)_energy / fullValue;
            ratio = Mathf.Clamp01(ratio);
            transform.localScale = new Vector3(originScale.x * ratio, originScale.y, originScale.z);

        }).AddTo(gameObject);
        transform.localScale = originScale;
    }

    public void Init(ReactiveProperty<int> progress, int fullValue) {
        sedi.Disposable = progress.Skip(1).Subscribe(_energy => {
            var ratio = (float)_energy / fullValue;
            ratio = Mathf.Clamp01(ratio);
            transform.localScale = new Vector3(originScale.x * ratio, originScale.y, originScale.z);

        }).AddTo(gameObject);
        transform.localScale = originScale;
    }
}
