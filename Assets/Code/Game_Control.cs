using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using Sirenix.OdinInspector;
using System.Threading;
using UniRx;
using UnityEngine.SceneManagement;

public class Game_Control : MonoBehaviour
{
    private static Game_Control _singleton;
    public static Game_Control game_control=> _singleton;

    [ShowInInspector]
    Game_State game_state => Static_Game_Scope.game_state;

    private CancellationTokenSource _cancel = null;
    private CompositeDisposable _com = null;

    public static CancellationToken SceneLifetimeCancelToken => _singleton._cancel.Token;
    public static CompositeDisposable SceneLifetimeDisposable => _singleton.CompositeDisposableOnExitScene;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        _cancel = new CancellationTokenSource();

        //mmFaderRound.SendMessage("Awake");
    }

    private void OnSceneUnloaded(Scene scene) {
        SafeCancellationDispose();

        Time.timeScale = 1;
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


    public static async UniTask Make_Instance() {
        var handle = Addressables.InstantiateAsync("Game_Control");
        await handle.Task;
        Assert.AreEqual(handle.Status, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded, "資源載入出現錯誤");

        handle.Result.TryGetComponent(out _singleton);
    }

    void Awake() {
        DontDestroyOnLoad(gameObject);

    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Static_Game_Scope.game_state.running.Value ^= true;
            Time.timeScale = (Static_Game_Scope.game_state.running.Value) ? 1 : 0;
        }
    }


}