using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class Cheat_Helper : EditorWindow
{
    [MenuItem("My Game/Play \u2044 Time Scale _F1")]
    public static void ToggleTime() {
        if (EditorApplication.isPlaying) {
            GCQ.IGame_Scope.game_data.running.Value ^= true;
            Time.timeScale = (GCQ.IGame_Scope.game_data.running.Value) ? 1 : 0;
            Debug.LogWarning(Time.timeScale);

        }
    }


    [MenuItem("My Game/Main Game _F2")]
    public static void NewMainGameWindow() {
        if (EditorApplication.isPlaying) {
            var runtime = FindObjectOfType<Game_Control>();
            Selection.activeGameObject = runtime.gameObject;
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            GetWindow(type, false, "static inspector", true);
        }
        else {
            var resource = AssetDatabase.LoadMainAssetAtPath("Assets/Prefab/Game_Control.prefab");
            Selection.activeObject = resource;
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            GetWindow(type, false, "static inspector", true);
        }
    }

    [MenuItem("My Game/Battlefield Control _F3")]
    public static void NewBattleFieldControlWindow() {
        if (EditorApplication.isPlaying) {
            var runtime = FindObjectOfType<Battle_Scene>();
            Selection.activeGameObject = runtime.gameObject;
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            GetWindow(type, false, "static inspector", true);
        }
    }

    [MenuItem("My Game/DataManager _F4")]
    public static void DataManager() {
        if (EditorApplication.isPlaying) {
            var runtime = FindObjectOfType<Data_Manager>();
            Selection.activeGameObject = runtime.gameObject;
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            GetWindow(type, false, "static inspector", true);
        }
        else {
            var resource = AssetDatabase.LoadMainAssetAtPath("Assets/Prefab/Data_Manager.prefab");
            Selection.activeObject = resource;
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            GetWindow(type, false, "static inspector", true);
        }
    }

    [MenuItem("My Game/Play \u2044 Pause _F5")]
    public static void Pause() {
        if (EditorApplication.isPlaying) {
            if (!EditorApplication.isPaused)
                Debug.Break();
            else
                EditorApplication.isPaused = false;
        }
        else
            EditorApplication.EnterPlaymode();
    }

    [MenuItem("My Game/Reload #F5")]
    public static void ReloadDomain() {
        if (EditorApplication.isPlaying)
            return;

        EditorUtility.RequestScriptReload();
        EditorApplication.EnterPlaymode();
    }

    [MenuItem("My Game/Stop _F7")]
    public static void Stop() {
        if (EditorApplication.isPlaying)
            EditorApplication.ExitPlaymode();
    }

    [MenuItem("My Game/Cheats _F8")]
    public static void NewCheatWindow() {
        StaticInspectorWindow.InspectType(typeof(Cheat_Code), StaticInspectorWindow.AccessModifierFlags.All, StaticInspectorWindow.MemberTypeFlags.AllButObsolete);
    }

}