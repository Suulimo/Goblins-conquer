using Cysharp.Threading.Tasks;
using UnityEngine;
using static Data_Manager;

//public readonly struct PoolObj_GameObj
//{
//    public readonly UniRx_Pool_Component poolObj;
//    public readonly GameObject gameObj;

//    PoolObj_GameObj(UniRx_Pool_Component po, GameObject ga) {
//        poolObj = po;
//        gameObj = ga;
//    }

//    public void Deconstruct(out UniRx_Pool_Component po, out GameObject ga) {
//        po = poolObj;
//        ga = gameObj;
//    }

//    public static implicit operator PoolObj_GameObj((UniRx_Pool_Component po, GameObject ga) x) => new PoolObj_GameObj(x.po, x.ga);
//    public static implicit operator GameObject(PoolObj_GameObj x) => x.gameObj;
//    //public static explicit operator UniRx_Pool_Component (PoolObj_GameObj x) => x.poolObj;
//}

public static class Rent_System
{
    public static T Spawn<T>(string key, Transform parent) where T : MonoBehaviour {
        var prefab = data_manager.prefabDict[key].prefab;

        if (prefab == null) {
            Debug.LogWarning("spawn dict hasn't got key: " + key);
            return null;
        }

        if (GameObject.Instantiate(prefab, parent).TryGetComponent<T>(out T tt))
            return tt;

        return null;
    }


    public static GameObject Spawn(string key, Transform parent) {
        if (data_manager.prefabDict.TryGetValue(key, out var value) == false || value.prefab == null) {
            Debug.LogWarning("spawn dict hasn't got key: " + key);
            return null;
        }

        return GameObject.Instantiate(value.prefab, parent);
    }

    public static Pawn_Monobe Rent_Pawn_Monobe(string key, Transform parent = null, bool worldPositionStays = true) {

        var pool = data_manager.pawn_monobe_pool;

        if (pool == null) {
            Debug.LogWarning("[Rent] object pool hasn't got key: " + key);
            return Spawn<Pawn_Monobe>(key, parent);
        }

        var pObj = pool.Rent();

        // parent不指定的話，繼續掛在manager下
        if (parent != null)
            pObj.transform.SetParent(parent, worldPositionStays);

        return pObj;
    }

    public static Slot_Display_Monobe Rent_Slot_Display_Monobe(string key, Transform parent = null, bool worldPositionStays = true) {

        var pool = data_manager.slot_display_monobe_pool;

        if (pool == null) {
            Debug.LogWarning("[Rent] object pool hasn't got key: " + key);
            return Spawn<Slot_Display_Monobe>(key, parent);
        }

        var pObj = pool.Rent();

        // parent不指定的話，繼續掛在manager下
        if (parent != null)
            pObj.transform.SetParent(parent, worldPositionStays);

        return pObj;
    }

    public static void Return_Self(this Pawn_Monobe gObj) {
        Return_Pawn_Monobe(gObj);
    }

    public static void Return_Pawn_Monobe(Pawn_Monobe gObj) {

        var pool = data_manager.pawn_monobe_pool;
        if (pool == null) {
            Debug.LogWarning("[Return] object pool hasn't got key: " + gObj.name, gObj);
            Object.Destroy(gObj);
        }

        pool.Return(gObj);
    }

    public static void Return_Self(this Slot_Display_Monobe gObj) {
        Return_Slot_Display_Monobe(gObj);
    }

    public static void Return_Slot_Display_Monobe(Slot_Display_Monobe gObj) {

        var pool = data_manager.slot_display_monobe_pool;
        if (pool == null) {
            Debug.LogWarning("[Return] object pool hasn't got key: " + gObj.name, gObj);
            Object.Destroy(gObj);
        }

        pool.Return(gObj);
    }

}
