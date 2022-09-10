using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UniRx;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR

public static class Cheat_Code
{
    [DisableInEditorMode]
    public static void 重新開始() {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Battle Scene");
    }

    [DisableInEditorMode]
    public static void TimeScale(float ts = 1) {
        Time.timeScale = ts;
    }

    [DisableInEditorMode]
    public static void Time_1x() {
        Time.timeScale = 1;
    }

    [DisableInEditorMode]
    public static void Time_2x() {
        Time.timeScale = 2;
    }

    [DisableInEditorMode]
    public static void Time_4x() {
        Time.timeScale = 4;
    }

    [DisableInEditorMode]
    public static void Time_10x() {
        Time.timeScale = 10;
    }

    [DisableInEditorMode]
    public static void Buy_Goblin_x1() {
        Battle_Sys.Spawn_Goblin_Random(Random.Range(1, 4), Static_Game_Scope.battle_scope);
    }

    [DisableInEditorMode]
    public static void Buy_Goblin_x5() {
        Battle_Sys.Spawn_Goblin_Random(Random.Range(1, 4), Static_Game_Scope.battle_scope);
        Battle_Sys.Spawn_Goblin_Random(Random.Range(1, 4), Static_Game_Scope.battle_scope);
        Battle_Sys.Spawn_Goblin_Random(Random.Range(1, 4), Static_Game_Scope.battle_scope);
        Battle_Sys.Spawn_Goblin_Random(Random.Range(1, 4), Static_Game_Scope.battle_scope);
        Battle_Sys.Spawn_Goblin_Random(Random.Range(1, 4), Static_Game_Scope.battle_scope);
    }

    [DisableInEditorMode]
    public static void Buy_Human_x1() {
        Battle_Sys.Spawn_Human_Random(Static_Game_Scope.battle_scope);
    }

    [DisableInEditorMode]
    public static void Buy_Human_x5() {
        Battle_Sys.Spawn_Human_Random(Static_Game_Scope.battle_scope);
        Battle_Sys.Spawn_Human_Random(Static_Game_Scope.battle_scope);
        Battle_Sys.Spawn_Human_Random(Static_Game_Scope.battle_scope);
        Battle_Sys.Spawn_Human_Random(Static_Game_Scope.battle_scope);
        Battle_Sys.Spawn_Human_Random(Static_Game_Scope.battle_scope);
    }


    //public static void UnloadUnuse() {
    //    Resources.UnloadUnusedAssets();
    //}

    //[FoldoutGroup("Common UI"), Indent]
    //public static async void SettingUI() {
    //}

    //public static void ClearInventory() {
    //}

    //public static void 無敵1000秒() {
    //    if (!EditorApplication.isPlaying)
    //        return;
    //}
}

#endif