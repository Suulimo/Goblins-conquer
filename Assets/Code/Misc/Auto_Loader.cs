using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Auto_Loader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    /// <summary>加載應用程序管理器</summary>
    private static async UniTask LoadAppManager() {
        Debug.Log("Auto_Loader Before scene loaded");

        Application.targetFrameRate = 60;


        if (Data_Manager.data_manager == null) {
            await Data_Manager.Make_Instance();
        }

        if (Game_Control.game_control == null) {
            await Game_Control.Make_Instance();
        }

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
