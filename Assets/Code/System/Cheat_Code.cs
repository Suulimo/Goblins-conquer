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

    public static void TimeScale(float ts = 1) {
        Time.timeScale = ts;
    }

    public static void Win() {
    }

    public static void Lose() {
    }

    public static void Hp0() {
    }

    public static void Revive() {
    }

    public static void ClearLevel() {
    }

    public static void UnloadUnuse() {
        Resources.UnloadUnusedAssets();
    }

    [FoldoutGroup("Common UI"), Indent]
    public static async void SettingUI() {
    }

    [FoldoutGroup("Common UI"), Indent]
    public static async void CatBoxPageUI() {
    }

    [FoldoutGroup("Common UI"), Indent]
    public static async void CatBoxPageUnlockUI() {
    }

    [FoldoutGroup("Common UI"), Indent]
    public static async void UIRareReward() {
    }

    [FoldoutGroup("Common UI"), Indent]
    public static async void UIRoomDecoReward() {
    }

    [FoldoutGroup("Common UI"), Indent]
    public static async void CatLibraryPageUI() {
    }

    [FoldoutGroup("Common UI"), Indent]
    public static async void CatInfoPageUI() {
    }

    [FoldoutGroup("Common UI"), Indent]
    public static async void CatLevelUpPageUI() {
    }

    [FoldoutGroup("Common UI"), Indent]
    public static void QteColor() {
    }

    public static void CoinParticle() {
    }

    public static void GroundBreakParticle() {
    }


    public static void ClearInventory() {
    }

    public static void ClearLibrary() {
    }

    public static void Add10000Medal() {
    }

    public static void Add10000Coin() {
    }

    public static void Add10000Gem() {
    }

    static public void PlaySfx(Sound_Id sid = Sound_Id.Click_Bottun) {
        //AudioSystem.PlaySfx(sid);
    }

    static public void PlayMusic(Music_Id mid = Music_Id.Title_Scene) {
        //AudioSystem.PlayMusic(mid);
    }

    public static void 無敵1000秒() {
        if (!EditorApplication.isPlaying)
            return;
    }
}

#endif