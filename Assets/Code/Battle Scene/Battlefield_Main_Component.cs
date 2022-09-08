using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using static Static_Game_Scope;

public class Battlefield_Main_Component : MonoBehaviour
{
    public Battlefield_Slot_Component[] a_group;
    public Battlefield_Slot_Component[] b_group;
    public Battlefield_Slot_Component[] c_group;

    public SpriteRenderer dragged;

    Battlefield_Use battlefield_use = null;



    void Awake() {
        battlefield_use = new Battlefield_Use(this);
    }

    // Start is called before the first frame update
    async UniTaskVoid Start() {

        await UniTask.WaitUntil(() => Game_Control.game_control != null);

        Game_Control.game_control.Hack_Ask_Position = battlefield_use.Get_Slot_Position;
        Game_Control.game_control.Hack_Ask_Slot = battlefield_use.Get_Slot_Info;
        Game_Control.game_control.Hack_Scan_Target = battlefield_use.Scan_Target;
        Game_Control.game_control.Hack_Pawn_Die = battlefield_use.On_Pawn_Die;

        battlefield_use.Start_My_Update();
    }

    void OnDestroy() {
        Game_Control.game_control.Hack_Ask_Position = null;// Get_Slot_Position;
        Game_Control.game_control.Hack_Ask_Slot = null;// Get_Slot_Info;
        Game_Control.game_control.Hack_Scan_Target = null;// Scan_Target;
        Game_Control.game_control.Hack_Pawn_Die = null;
    }
}