using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using static Static_Game_Scope;

public class Battlefield_Main_Component : MonoBehaviour
{
    public Battlefield_Slot_Component[] a_group;
    public Battlefield_Slot_Component[] b_group;
    public Battlefield_Slot_Component[] c_group;

    public SpriteRenderer dragged;

    public Tilemap tilemap;
    public GameObject slot_clone;

    Battlefield_Use battlefield_use = null;


    void Awake() {
        battlefield_use = new Battlefield_Use(this);

        tilemap.GetComponent<TilemapRenderer>().enabled = false;
    }

    // Start is called before the first frame update
    async UniTaskVoid Start() {

        await UniTask.WaitUntil(() => Game_Control.game_control != null);

        Game_Control.game_control.Hack_Ask_Position = battlefield_use.Get_Slot_Position;
        Game_Control.game_control.Hack_Ask_Slot = battlefield_use.Get_Slot_Info;
        Game_Control.game_control.Hack_Scan_Target = battlefield_use.Scan_Target;
        Game_Control.game_control.Hack_Pawn_Die = battlefield_use.On_Pawn_Die;

        await UniTask.WaitUntil(() => Data_Manager.data_manager.Is_Pool_PreLoading_OK);

        var setting = Data_Manager.data_manager.temp_game_setting;
        for (int i = 0; i < setting.ally_start_num; i++) {
            Battle_Sys.Spawn_Human_Random(Static_Game_Scope.battle_scope);
        }
        for (int i = 0; i < setting.enemy_start_num; i++) {
            Battle_Sys.Spawn_Goblin_Random(Random.Range(1, 4), Static_Game_Scope.battle_scope);
        }
        for (int i = 0; i < setting.bed_start_num; i++) {
            Battle_Sys.Spawn_Bed_Random(Random.Range(1, 3), Random.Range(0, 6), Static_Game_Scope.battle_scope);
        }


        battlefield_use.Start_My_Update();
    }

    void OnDestroy() {
        Game_Control.game_control.Hack_Ask_Position = null;// Get_Slot_Position;
        Game_Control.game_control.Hack_Ask_Slot = null;// Get_Slot_Info;
        Game_Control.game_control.Hack_Scan_Target = null;// Scan_Target;
        Game_Control.game_control.Hack_Pawn_Die = null;
    }

    public (int n1, int n2, int n3) InitMap() {
        return battlefield_use.MakeMap(tilemap, slot_clone);
    }
}