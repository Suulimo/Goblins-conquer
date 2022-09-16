using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.LowLevel;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Auto_Loader
{
    // AfterAssembliesLoaded is called before BeforeSceneLoad
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void InitUniTaskLoop() {
        var loop = PlayerLoop.GetCurrentPlayerLoop();
        Cysharp.Threading.Tasks.PlayerLoopHelper.Initialize(ref loop);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    /// <summary>加載應用程序管理器</summary>
    private static void LoadAppManager() {
        Debug.Log("Auto_Loader Before scene loaded");

        Application.targetFrameRate = 500;

        if (Data_Manager.data_manager == null) {
            Data_Manager.Make_Instance_Sync();
        }

        if (Game_Control.game_control == null) {
            Game_Control.Make_Instance_Sync();
        }

        Assert.IsTrue(Data_Manager.UserReady());

        Debug.Log("Auto_Loader Before scene loaded await end");
    }

#if UNITY_EDITOR
    [MenuItem("File/Develop Mode/On")]
    static void EditorDeveloperModeOn() {
        UnityEditor.EditorPrefs.SetBool("DeveloperMode", true);
    }
    [MenuItem("File/Develop Mode/Off")]
    static void EditorDeveloperModeOff() {
        UnityEditor.EditorPrefs.SetBool("DeveloperMode", false);
    }
#endif

}
