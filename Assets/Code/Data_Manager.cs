using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UniRx;
using UniRx.Toolkit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

public class Data_Manager : MonoBehaviour
{
    private static Data_Manager _singleton;
    public bool Is_Pool_PreLoading_OK { get; private set; }

    static bool async_init = false;

    [SerializeField]
    Prefab_Table prefab_table;

    public Share_Pic share_pic;
    public Share_Audio share_audio;
    public Temp_Game_Setting temp_game_setting;
    public Human_Spec_File human_data_file;
    public Goblin_Spec_File goblin_data_file;
    public Goblin_Spec_File special_goblin_data_file;

    public bool use_human_data_file = true;
    public bool use_goblin_data_file = true;

    public Dictionary<string, (GameObject prefab, int initPoolCapacity)> prefabDict;

    Pawn_Monobe pawn_monobe_prefab;
    Slot_Display_Monobe slot_display_monobe_prefab;

    public Pawn_Monobe_Pool pawn_monobe_pool { get; private set; }
    public Slot_Display_Monobe_Pool slot_display_monobe_pool { get; private set; }

    // 單例管理類，取個明確的名字，方便using static，縮短code
    public static Data_Manager data_manager {
        get { return _singleton; }
    }

    public static bool Data_Manager_Ready() {
        return _singleton != null;
    }

    public static bool UserReady() {
        return _singleton != null && _singleton.Is_Pool_PreLoading_OK;
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
            GCQ.Babamama_Sys.Init_Table(_singleton.special_goblin_data_file.goblin_list.ToArray());
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
            GCQ.Babamama_Sys.Init_Table(_singleton.special_goblin_data_file.goblin_list.ToArray());
        }
    }

    void Awake() {
        DontDestroyOnLoad(gameObject);

        _ = Load_Prefab_Async(async_init);

    }

    async UniTaskVoid Load_Prefab_Async(bool is_async) {
        if (Is_Pool_PreLoading_OK == true)
            return;

        prefabDict = new Dictionary<string, (GameObject, int)>();
        for (int i = 0; i < prefab_table.sheet.Count; i++) {
            var p = prefab_table.sheet[i];

            GameObject prefab = null;
            if (is_async) {
                var handle = Addressables.LoadAssetAsync<GameObject>(p.key);
                await handle.Task;
                prefab = handle.Result;
            }
            else {
                prefab = Addressables.LoadAssetAsync<GameObject>(p.key).WaitForCompletion();
            }

            prefabDict.Add(p.key, (prefab, p.init_pool_capacity));
            Debug.Log($"{p.key} ({p.init_pool_capacity})");

            List<UniTask> preload_task = new ();

            switch (p.key) {
                case "Pawn":
                    prefab.TryGetComponent<Pawn_Monobe>(out pawn_monobe_prefab);
                    if (p.init_pool_capacity > 0) {

                        if (is_async) {
                            var parentObj = new GameObject(p.key);
                            parentObj.transform.SetParent(this.transform);
                            pawn_monobe_pool = new Pawn_Monobe_Pool(parentObj.transform, pawn_monobe_prefab);
                            var task = pawn_monobe_pool.PreloadAsync(p.init_pool_capacity, 8).DoOnCompleted(() => {
                                Debug.Log("張終了 " + p.key);
                            }).ToUniTask();
                            preload_task.Add(task);
                        }
                        else {
                            Stack<Pawn_Monobe> stack = new Stack<Pawn_Monobe>();

                            var parentObj = new GameObject(p.key);
                            parentObj.transform.SetParent(this.transform);
                            pawn_monobe_pool = new Pawn_Monobe_Pool(parentObj.transform, pawn_monobe_prefab);

                            for (int j = 0; j < p.init_pool_capacity; j++) {
                                var a = pawn_monobe_pool.Rent();
                                stack.Push(a);
                            }

                            for (int j = 0; j < p.init_pool_capacity; j++) {
                                var a = stack.Pop();
                                pawn_monobe_pool.Return(a);
                            }

                            Assert.IsTrue(stack.Count == 0);

                            Debug.Log("張終了 " + p.key + " " + pawn_monobe_pool.Count);
                        }
                    }
                    break;
                case "Temp Slot Display":
                    prefab.TryGetComponent(out slot_display_monobe_prefab);
                    if (p.init_pool_capacity > 0) {

                        if (is_async) {
                            var parentObj = new GameObject(p.key);
                            parentObj.transform.SetParent(this.transform);
                            slot_display_monobe_pool = new Slot_Display_Monobe_Pool(parentObj.transform, slot_display_monobe_prefab);
                            var task = slot_display_monobe_pool.PreloadAsync(p.init_pool_capacity, 8).DoOnCompleted(() => {
                                Debug.Log("張終了 " + p.key);
                            }).ToUniTask();
                            preload_task.Add(task);
                        }
                        else {
                            Stack<Slot_Display_Monobe> stack = new Stack<Slot_Display_Monobe>();

                            var parentObj = new GameObject(p.key);
                            parentObj.transform.SetParent(this.transform);
                            slot_display_monobe_pool = new Slot_Display_Monobe_Pool(parentObj.transform, slot_display_monobe_prefab);

                            for (int j = 0; j < p.init_pool_capacity; j++) {
                                var a = slot_display_monobe_pool.Rent();
                                stack.Push(a);
                            }

                            for (int j = 0; j < p.init_pool_capacity; j++) {
                                var a = stack.Pop();
                                slot_display_monobe_pool.Return(a);
                            }

                            Assert.IsTrue(stack.Count == 0);

                            Debug.Log("張終了 " + p.key + " " + slot_display_monobe_pool.Count);
                        }
                    }
                    break;
                default:
                    break;
            }

            if (is_async)
                await UniTask.WhenAll(preload_task);

        }
        Is_Pool_PreLoading_OK = true;
    }

    public void Force_Return_Pool() {
        pawn_monobe_pool.Return_Keep_All();
    }

}
