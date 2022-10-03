using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR

public static class Cheat_Code
{
    [DisableInEditorMode]
    public static void 重新開始() {
        UniTask.Void(async () => {
            await UniTask.SwitchToMainThread();
            Game_Control.game_control.SafeCancellationDispose();
            Data_Manager.data_manager.Force_Return_Pool();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Battle Scene");
            GCQ.IGame_Scope.Restart();
        });
    }

    [DisableInEditorMode]
    public static void Title() {
        UniTask.Void(async () => {
            await UniTask.SwitchToMainThread();
            Game_Control.game_control.SafeCancellationDispose();
            Data_Manager.data_manager.Force_Return_Pool();
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        });
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
        GCQ.Battle_Sys.Spawn_Goblin_Random(GCQ.IGame_Scope.battle_scope, Random.Range(1, 4));
    }

    [DisableInEditorMode]
    public static void Buy_Goblin_x5() {
        GCQ.Battle_Sys.Spawn_Goblin_Random(GCQ.IGame_Scope.battle_scope, Random.Range(1, 4));
        GCQ.Battle_Sys.Spawn_Goblin_Random(GCQ.IGame_Scope.battle_scope, Random.Range(1, 4));
        GCQ.Battle_Sys.Spawn_Goblin_Random(GCQ.IGame_Scope.battle_scope, Random.Range(1, 4));
        GCQ.Battle_Sys.Spawn_Goblin_Random(GCQ.IGame_Scope.battle_scope, Random.Range(1, 4));
        GCQ.Battle_Sys.Spawn_Goblin_Random(GCQ.IGame_Scope.battle_scope, Random.Range(1, 4));
    }

    [DisableInEditorMode]
    public static void Buy_Human_x1() {
        GCQ.Battle_Sys.Spawn_Human_Random(GCQ.IGame_Scope.battle_scope);
    }

    [DisableInEditorMode]
    public static void Buy_Human_x5() {
        GCQ.Battle_Sys.Spawn_Human_Random(GCQ.IGame_Scope.battle_scope);
        GCQ.Battle_Sys.Spawn_Human_Random(GCQ.IGame_Scope.battle_scope);
        GCQ.Battle_Sys.Spawn_Human_Random(GCQ.IGame_Scope.battle_scope);
        GCQ.Battle_Sys.Spawn_Human_Random(GCQ.IGame_Scope.battle_scope);
        GCQ.Battle_Sys.Spawn_Human_Random(GCQ.IGame_Scope.battle_scope);
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