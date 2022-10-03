using System.Collections.Generic;
using UniRx.Toolkit;
using UnityEngine;
using UnityEngine.Assertions;

public class Slot_Display_Monobe_Pool : ObjectPool<Slot_Display_Monobe>
{
    private readonly Slot_Display_Monobe prefab;
    private readonly Transform parent_transform;
    private HashSet<Slot_Display_Monobe> all_rents = new HashSet<Slot_Display_Monobe>();

    //コンストラクタ
    public Slot_Display_Monobe_Pool(Transform parent_transform, Slot_Display_Monobe prefab) {
        this.parent_transform = parent_transform;
        this.prefab = prefab;
    }

    /// <summary>
    /// オブジェクトの追加生成時に実行される
    /// </summary>
    protected override Slot_Display_Monobe CreateInstance() {
        //新しく生成
        var e = GameObject.Instantiate(prefab);

        //ヒエラルキーが散らからないように一箇所にまとめる
        e.transform.SetParent(parent_transform);

        return e;
    }

    /// <summary>
    /// オブジェクトの貸出時に実行される
    /// </summary>
    protected override void OnBeforeRent(Slot_Display_Monobe instance) {
        all_rents.Add(instance);

        //貸し出すオブジェクトのインスタンスIDを出力
        //Debug.Log(instance.GetInstanceID());

        //baseではinstance.gameObject.SetActive(true)を実行している
        base.OnBeforeRent(instance);

        instance.transform.position = Vector3.zero;
    }

    protected override void OnBeforeReturn(Slot_Display_Monobe instance) {
        all_rents.Remove(instance);

        instance.transform.position = Vector3.zero;

        instance.transform.SetParent(parent_transform);

        //baseではinstance.gameObject.SetActive(false)を実行している
        base.OnBeforeReturn(instance);

        //返却されたオブジェクトのインスタンスIDを出力
        //Debug.Log(instance.GetInstanceID());
    }

    protected override void OnClear(Slot_Display_Monobe instance) {
        //削除オブジェクトのインスタンスIDを出力
        //Debug.Log(instance.GetInstanceID());

        //baseでDestoryされる
        base.OnClear(instance);
    }


    public void Return_Keep_All() {
        if (all_rents.Count <= 0)
            return;
        var set2 = new HashSet<Slot_Display_Monobe>(all_rents);
        foreach (var r in set2) {
            try { this.Return(r); }
            catch { }
        }
        Assert.AreEqual(all_rents.Count, 0, "回收物件殘留");

        all_rents.Clear();
    }
}