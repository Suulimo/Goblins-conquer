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

    public Dictionary<string, (GameObject prefab, int initPoolCapacity)> prefabDict;
    public Dictionary<string, (GameObject prefab, int initPoolCapacity)> monsterPrefabDict;
    public Dictionary<string, UniRx_Pool> pools;

    // ��Һ޲z���A���ө��T���W�r�A��Kusing static�A�Y�ucode
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
        Assert.AreEqual(handle.Status, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded, "UI�귽���J�X�{���~");

        handle.Result.TryGetComponent(out _singleton);
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
                Debug.Log("�i�פF " + p.Key);

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
