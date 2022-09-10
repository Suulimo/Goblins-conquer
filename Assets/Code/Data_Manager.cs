using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class Data_Manager : MonoBehaviour
{
    private static Data_Manager _singleton;
    private bool prefab_oK;
    public bool Is_Pool_PreLoading_OK { get; private set; }

    [SerializeField]
    Prefab_Table prefab_table;
    
    public Share_Pic share_pic;
    public Share_Audio share_audio;
    public Temp_Game_Setting temp_game_setting;
    public Human_Data_File human_data_file;
    public Goblin_Data_File goblin_data_file;

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

    public static async UniTask Make_Instance() {
        var handle = Addressables.InstantiateAsync("Data_Manager");
        await handle.Task;
        Assert.AreEqual(handle.Status, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded, "UI資源載入出現錯誤");

        handle.Result.TryGetComponent(out _singleton);

        if (_singleton.use_human_data_file && _singleton.human_data_file != null) {
            Human_Def.Default_Human_List = _singleton.human_data_file.female_list.ToArray();
            Human_Def.Default_Male_Human_List = _singleton.human_data_file.male_list.ToArray();
        }
        if (_singleton.use_goblin_data_file && _singleton.goblin_data_file != null) {
            Goblin_Def.Default_Goblin_List = _singleton.goblin_data_file.goblin_list.ToArray();
        }

    }

    async UniTaskVoid Awake() {
        DontDestroyOnLoad(gameObject);

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

        //AudioSystem.SetMusic(curGameOption.isMusic);
        //AudioSystem.SetSound(curGameOption.isSound);
        //AudioSystem.BindGameSetting();

    }

    async UniTaskVoid Start() {
        await UniTask.WaitUntil(() => prefab_oK);

        pools = new Dictionary<string, UniRx_Pool>();
        foreach (var p in prefabDict) {
            if (p.Value.initPoolCapacity > 0) {
                UniRx_Pool_Component component = p.Value.prefab.GetComponent<UniRx_Pool_Component>();

                if (component == null)
                    component = p.Value.prefab.AddComponent<UniRx_Pool_Component>();

                component.poolKey = p.Key;

                var parentObj = new GameObject(p.Key);
                parentObj.transform.SetParent(_singleton.transform);
                var pool = new UniRx_Pool(parentObj.transform, component);

                await pool.PreloadAsync(p.Value.initPoolCapacity, 8);
                Debug.Log("張終了 " + p.Key);

                pools.Add(p.Key, pool);
            }
        }

        Is_Pool_PreLoading_OK = true;

        SceneManager.sceneUnloaded += Return_All_Rents;
    }

    private void OnDestroy() {
        SceneManager.sceneUnloaded -= Return_All_Rents;
    }

    private void Return_All_Rents(Scene scene) {
        foreach (var pool in pools) {
            pool.Value.Return_Keep_All();
        }
    }

    public void Force_Return_Pool() {
        foreach (var pool in pools) {
            pool.Value.Return_Keep_All();
        }
    }

}
