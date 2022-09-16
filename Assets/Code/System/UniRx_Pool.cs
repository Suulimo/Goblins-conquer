using System.Collections.Generic;
using UniRx.Toolkit;
using UnityEngine;
using UnityEngine.Assertions;

public class UniRx_Pool : ObjectPool<UniRx_Pool_Component>
{
    private readonly UniRx_Pool_Component prefab;
    private readonly Transform parent_transform;
    private HashSet<UniRx_Pool_Component> all_rents = new HashSet<UniRx_Pool_Component>();

    //コンストラクタ
    public UniRx_Pool(Transform parent_transform, UniRx_Pool_Component prefab) {
        this.parent_transform = parent_transform;
        this.prefab = prefab;
    }

    /// <summary>
    /// オブジェクトの追加生成時に実行される
    /// </summary>
    protected override UniRx_Pool_Component CreateInstance() {
        //新しく生成
        var e = GameObject.Instantiate(prefab);

        //ヒエラルキーが散らからないように一箇所にまとめる
        e.transform.SetParent(parent_transform);

        return e;
    }

    /// <summary>
    /// オブジェクトの貸出時に実行される
    /// </summary>
    protected override void OnBeforeRent(UniRx_Pool_Component instance) {
        all_rents.Add(instance);

        //貸し出すオブジェクトのインスタンスIDを出力
        //Debug.Log(instance.GetInstanceID());

        //baseではinstance.gameObject.SetActive(true)を実行している
        base.OnBeforeRent(instance);

        switch (instance.type) {
            case UniRx_Pool_Type.Goblin:
                var g_pawn_control = instance.GetComponent<Pawn_Monobe>();
                g_pawn_control.goblin_pawn = null;
                g_pawn_control.human_pawn = null;
                instance.transform.position = Vector3.zero;
                break;

            case UniRx_Pool_Type.Human:
                var h_pawn_control = instance.GetComponent<Pawn_Monobe>();
                h_pawn_control.goblin_pawn = null;
                h_pawn_control.human_pawn = null;
                instance.transform.position = Vector3.zero;
                break;

            case UniRx_Pool_Type.None:
                instance.transform.position = Vector3.zero;
                break;
        }
    }

    protected override void OnBeforeReturn(UniRx_Pool_Component instance) {
        all_rents.Remove(instance);

        switch (instance.type) {
            case UniRx_Pool_Type.Goblin:
                var g_pawn_control = instance.GetComponent<Pawn_Monobe>();
                g_pawn_control.goblin_pawn = null;
                g_pawn_control.human_pawn = null;
                break;

            case UniRx_Pool_Type.Human:
                var h_pawn_control = instance.GetComponent<Pawn_Monobe>();
                h_pawn_control.goblin_pawn = null;
                h_pawn_control.human_pawn = null;
                instance.transform.position = Vector3.zero;
                break;

            case UniRx_Pool_Type.None:
                break;
        }

        instance.transform.SetParent(parent_transform);

        //baseではinstance.gameObject.SetActive(false)を実行している
        base.OnBeforeReturn(instance);

        //返却されたオブジェクトのインスタンスIDを出力
        //Debug.Log(instance.GetInstanceID());
    }

    protected override void OnClear(UniRx_Pool_Component instance) {
        //削除オブジェクトのインスタンスIDを出力
        //Debug.Log(instance.GetInstanceID());

        //baseでDestoryされる
        base.OnClear(instance);
    }


    public void Return_Keep_All() {
        if (all_rents.Count <= 0)
            return;
        var set2 = new HashSet<UniRx_Pool_Component>(all_rents);
        foreach (var r in set2) {
            try { this.Return(r); }
            catch { }
        }
        Assert.AreEqual(all_rents.Count, 0, "回收物件殘留");

        all_rents.Clear();
    }
}