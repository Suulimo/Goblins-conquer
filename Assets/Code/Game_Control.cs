using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class Game_Control : MonoBehaviour
{
    private static Game_Control _singleton;
    public static Game_Control game_control => _singleton;

    [ShowInInspector]
    GCQ.Game_Scope_Data game_state => GCQ.Static_Game_Scope.game_scope_data;

    private CancellationTokenSource _cancel = null;
    private CompositeDisposable _com = null;

    public static CancellationToken SceneLifetimeCancelToken => _singleton._cancel.Token;
    public static CompositeDisposable SceneLifetimeDisposable => _singleton.CompositeDisposableOnExitScene;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (_cancel == null)
            _cancel = new CancellationTokenSource();
    }

    private void OnSceneUnloaded(Scene scene) {
        //SafeCancellationDispose();
    }

    public void SafeCancellationDispose() {
        CallCancel();
        if (_com != null)
            _com.Clear();
    }

    private void CallCancel() {
        if (_cancel != null)
            _cancel.Cancel();
        _cancel?.Dispose();
        _cancel = null;
    }

    public CompositeDisposable CompositeDisposableOnExitScene {
        get {
            if (_com == null) {
                _com = new CompositeDisposable();
            }
            return _com;
        }
    }


    public static async UniTask Make_Instance_Async() {
        var handle = Addressables.InstantiateAsync("Game_Control");
        await handle.Task;
        Assert.AreEqual(handle.Status, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded, "�귽���J�X�{���~");

        handle.Result.TryGetComponent(out _singleton);
    }

    public static void Make_Instance_Sync() {
        var result = Addressables.InstantiateAsync("Game_Control").WaitForCompletion();

        result.TryGetComponent(out _singleton);
    }

    void Awake() {
        DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
        if (_cancel == null)
            _cancel = new CancellationTokenSource();
#endif

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            GCQ.Static_Game_Scope.game_scope_data.running.Value ^= true;
            Time.timeScale = (GCQ.Static_Game_Scope.game_scope_data.running.Value) ? 1 : 0;
        }
    }


}