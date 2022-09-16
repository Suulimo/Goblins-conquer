using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

public class Data_Manager : MonoBehaviour
{
    private static Data_Manager _singleton;
    private bool prefab_oK;
    public bool Is_Pool_PreLoading_OK { get; private set; }

    static bool async_init = false;

    [SerializeField]
    Prefab_Table prefab_table;

    public Share_Pic share_pic;
    public Share_Audio share_audio;
    public Temp_Game_Setting temp_game_setting;
    public Human_Spec_File human_data_file;
    public Goblin_Spec_File goblin_data_file;

    public bool use_human_data_file = true;
    public bool use_goblin_data_file = true;

    public Dictionary<string, (GameObject prefab, int initPoolCapacity)> prefabDict;
    public Dictionary<string, (GameObject prefab, int initPoolCapacity)> monsterPrefabDict;
    public Dictionary<string, UniRx_Pool> pools;

    // 單例管理類，取個明確的名字，方便using static，縮短code
    public static Data_Manager data_manager {
        get { return _singleton; }
    }

    public static bool Data_Manager_Ready() {
        return _singleton != null;
    }

    public static bool UserReady() {
        return _singleton != null && _singleton.prefab_oK;
    }

    public static async UniTask Make_Instance_Async() {
        async_init = true;

        var handle = Addressables.InstantiateAsync("Data_Manager");
        await handle.Task;
        Assert.AreEqual(handle.Status, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded, "UI資源載入出現錯誤");

        handle.Result.TryGetComponent(out _singleton);

        if (_singleton.use_human_data_file && _singleton.human_data_file != null) {
            GCQ.Human_Def.Default_Human_List = _singleton.human_data_file.female_list.ToArray();
            GCQ.Human_Def.Default_Male_Human_List = _singleton.human_data_file.male_list.ToArray();
        }
        if (_singleton.use_goblin_data_file && _singleton.goblin_data_file != null) {
            GCQ.Goblin_Def.Default_Goblin_List = _singleton.goblin_data_file.goblin_list.ToArray();
        }
    }

    public static void Make_Instance_Sync() {
        var result = Addressables.InstantiateAsync("Data_Manager").WaitForCompletion();

        result.TryGetComponent(out _singleton);

        if (_singleton.use_human_data_file && _singleton.human_data_file != null) {
            GCQ.Human_Def.Default_Human_List = _singleton.human_data_file.female_list.ToArray();
            GCQ.Human_Def.Default_Male_Human_List = _singleton.human_data_file.male_list.ToArray();
        }
        if (_singleton.use_goblin_data_file && _singleton.goblin_data_file != null) {
            GCQ.Goblin_Def.Default_Goblin_List = _singleton.goblin_data_file.goblin_list.ToArray();
        }
    }

    void Awake() {
        DontDestroyOnLoad(gameObject);

        if (async_init) {
            _ = Load_Prefab_Async();
            _ = Preload_Pool_Async();
        }
        else {
            Load_Prefab();
            Preload_Pool();
        }
    }

    async UniTaskVoid Load_Prefab_Async() {
        if (this.prefab_oK == true)
            return;

        prefabDict = new Dictionary<string, (GameObject, int)>();
        for (int i = 0; i < prefab_table.sheet.Count; i++) {
            var p = prefab_table.sheet[i];
            var handle = Addressables.LoadAssetAsync<GameObject>(p.key);
            await handle.Task;
            var prefab = handle.Result;
            if (prefab.TryGetComponent(out UniRx_Pool_Component component) == false) {
                component = prefab.AddComponent<UniRx_Pool_Component>();
            }
            prefabDict.Add(p.key, (prefab, p.init_pool_capacity));
            Debug.Log($"{p.key} ({p.init_pool_capacity})");
        }
        this.prefab_oK = true;
    }

    void Load_Prefab() {
        if (this.prefab_oK == true)
            return;

        prefabDict = new Dictionary<string, (GameObject, int)>();
        for (int i = 0; i < prefab_table.sheet.Count; i++) {
            var p = prefab_table.sheet[i];
            var prefab = Addressables.LoadAssetAsync<GameObject>(p.key).WaitForCompletion();
            if (prefab.TryGetComponent(out UniRx_Pool_Component component) == false) {
                component = prefab.AddComponent<UniRx_Pool_Component>();
            }
            prefabDict.Add(p.key, (prefab, p.init_pool_capacity));
            Debug.Log($"{p.key} ({p.init_pool_capacity})");
        }
        this.prefab_oK = true;
    }

    async UniTaskVoid Preload_Pool_Async() {
        if (Is_Pool_PreLoading_OK == true)
            return;

        await UniTask.WaitUntil(() => prefab_oK);

        pools = new Dictionary<string, UniRx_Pool>();
        foreach (var p in prefabDict) {
            if (p.Value.initPoolCapacity > 0) {
                UniRx_Pool_Component component = p.Value.prefab.GetComponent<UniRx_Pool_Component>();

                if (component == null)
                    component = p.Value.prefab.AddComponent<UniRx_Pool_Component>();

                component.poolKey = p.Key;

                var parentObj = new GameObject(p.Key);
                parentObj.transform.SetParent(this.transform);
                var pool = new UniRx_Pool(parentObj.transform, component);

                await pool.PreloadAsync(p.Value.initPoolCapacity, 8);
                Debug.Log("張終了 " + p.Key);

                pools.Add(p.Key, pool);
            }
        }

        Is_Pool_PreLoading_OK = true;
    }

    void Preload_Pool() {
        if (Is_Pool_PreLoading_OK == true)
            return;

        Assert.IsTrue(prefab_oK);

        Stack<UniRx_Pool_Component> stack = new Stack<UniRx_Pool_Component>();

        pools = new Dictionary<string, UniRx_Pool>();
        foreach (var p in prefabDict) {
            if (p.Value.initPoolCapacity > 0) {
                UniRx_Pool_Component component = p.Value.prefab.GetComponent<UniRx_Pool_Component>();

                if (component == null)
                    component = p.Value.prefab.AddComponent<UniRx_Pool_Component>();

                component.poolKey = p.Key;

                var parentObj = new GameObject(p.Key);
                parentObj.transform.SetParent(this.transform);
                var pool = new UniRx_Pool(parentObj.transform, component);

                for (int i = 0; i < p.Value.initPoolCapacity; i++) {
                    var a = pool.Rent();
                    stack.Push(a);
                }

                for (int i = 0; i < p.Value.initPoolCapacity; i++) {
                    var a = stack.Pop();
                    pool.Return(a);
                }

                Assert.IsTrue(stack.Count == 0);

                Debug.Log("張終了 " + p.Key + " " + pool.Count);

                pools.Add(p.Key, pool);
            }
        }

        Is_Pool_PreLoading_OK = true;
    }

    public void Force_Return_Pool() {
        foreach (var pool in pools) {
            pool.Value.Return_Keep_All();
        }
    }

}
