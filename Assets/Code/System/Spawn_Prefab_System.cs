using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Toolkit;
using Cysharp.Threading.Tasks;
using static Data_Manager;

public readonly struct PoolObj_GameObj
{
    public readonly UniRx_Pool_Component poolObj;
    public readonly GameObject gameObj;

    PoolObj_GameObj(UniRx_Pool_Component po, GameObject ga) {
        poolObj = po;
        gameObj = ga;
    }

    public void Deconstruct(out UniRx_Pool_Component po, out GameObject ga) {
        po = poolObj;
        ga = gameObj;
    }

    public static implicit operator PoolObj_GameObj((UniRx_Pool_Component po, GameObject ga) x) => new PoolObj_GameObj(x.po, x.ga);
    public static implicit operator GameObject(PoolObj_GameObj x) => x.gameObj;
    //public static explicit operator UniRx_Pool_Component (PoolObj_GameObj x) => x.poolObj;
}

public static class SpawnPrefabSystem
{
    public static GameObject Spawn(string key) {
        if (data_manager.prefabDict.TryGetValue(key, out var value) == false || value.prefab == null) {
            Debug.LogWarning("spawn dict hasn't got key: " + key);
            return null;
        }

        return GameObject.Instantiate(value.prefab);
    }

    public static GameObject SpawnMonster(string key) {
        if (data_manager.monsterPrefabDict.TryGetValue(key, out var value) == false || value.prefab == null) {
            Debug.LogWarning("monster spawn dict hasn't got key: " + key);
            return null;
        }

        return GameObject.Instantiate(value.prefab);
    }

    public static T Spawn<T>(string key) where T : MonoBehaviour {
        var prefab = data_manager.prefabDict[key].prefab;

        if (prefab == null) {
            Debug.LogWarning("spawn dict hasn't got key: " + key);
            return null;
        }

        return GameObject.Instantiate(prefab).GetComponent<T>();
    }


    public static GameObject Spawn(string key, Transform parent) {
        if (data_manager.prefabDict.TryGetValue(key, out var value) == false || value.prefab == null) {
            Debug.LogWarning("spawn dict hasn't got key: " + key);
            return null;
        }

        return GameObject.Instantiate(value.prefab, parent);
    }

    public static GameObject SpawnMonster(string key, Transform parent) {
        if (data_manager.monsterPrefabDict.TryGetValue(key, out var value) == false || value.prefab == null) {
            Debug.LogWarning("monster spawn dict hasn't got key: " + key);
            return null;
        }

        return GameObject.Instantiate(value.prefab, parent);
    }

    public static PoolObj_GameObj Rent(string key, Transform parent = null, bool worldPositionStays = true) {
        if (data_manager.pools.ContainsKey(key) == false) {
            Debug.LogWarning("[Rent] object pool hasn't got key: " + key);
            return (null, Spawn(key, parent));
        }

        var pool = data_manager.pools[key];

        if (pool == null) {
            Debug.LogWarning("[Rent] object pool hasn't got key: " + key);
            return (null, Spawn(key, parent));
        }

        var pObj = pool.Rent();

        // parent不指定的話，繼續掛在manager下
        if (parent != null)
            pObj.transform.SetParent(parent, worldPositionStays);

        return (pObj, pObj.gameObject);
    }

    public static PoolObj_GameObj RentMonster(string key, Transform parent = null, bool worldPositionStays = true) {
        if (data_manager.pools.ContainsKey(key) == false) {
            Debug.LogWarning("[Rent] object pool hasn't got key: " + key);
            return (null, SpawnMonster(key, parent));
        }

        var pool = data_manager.pools[key];

        if (pool == null) {
            Debug.LogWarning("[Rent] object pool hasn't got key: " + key);
            return (null, SpawnMonster(key, parent));
        }

        var pObj = pool.Rent();

        // parent不指定的話，繼續掛在manager下
        if (parent != null)
            pObj.transform.SetParent(parent, worldPositionStays);

        return (pObj, pObj.gameObject);
    }

    public static void SafeReturn(GameObject gObj) {
        if (gObj != null && gObj.activeSelf)
            Return(gObj);
    }

    static void Return(GameObject gObj) {
        // Rex Particle 動畫完就自己setActive false，讓他可以回收
        if (gObj.activeSelf == false) {
            Debug.LogWarning($"returning an inactive object : {gObj.name}", gObj);
            return;
        }

        var poolObj = gObj.GetComponent<UniRx_Pool_Component >();

        if (poolObj == null) {
            Debug.LogWarning($"returning a non-pool object : {gObj.name}", gObj);
            Object.Destroy(gObj);
            return;
        }

        if (data_manager.pools.ContainsKey(poolObj.poolKey) == false) {
            Debug.LogWarning("[Rent] object pool hasn't got key: " + poolObj.poolKey);
            Object.Destroy(gObj);
            return;
        }

        var pool = data_manager.pools[poolObj.poolKey];
        if (pool == null) {
            Debug.LogWarning("[Return] object pool hasn't got key: " + poolObj.poolKey, gObj);
            Object.Destroy(gObj);
        }

        if (poolObj.HasCompositeDisposable)
            poolObj.GetCompositeDisposableOnReturn.Clear();

        poolObj.CallCancel();

        pool.Return(poolObj);
    }

    public static async UniTaskVoid Return(GameObject gObj, float sec) {
        if (await UniTask.Delay((int)(sec * 1000), cancellationToken: Game_Control.SceneLifetimeCancelToken).SuppressCancellationThrow())
            return;
        SafeReturn(gObj);
    }

    public static async UniTaskVoid WillReturnInFrame(GameObject gObj, int frame) {
        if (await UniTask.DelayFrame(frame, cancellationToken: Game_Control.SceneLifetimeCancelToken).SuppressCancellationThrow())
            return;
        SafeReturn(gObj);
    }

}
