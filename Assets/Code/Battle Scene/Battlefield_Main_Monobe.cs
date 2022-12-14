using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Battlefield_Main_Monobe : MonoBehaviour
{
    public Battlefield_Slot_Monobe[] a_group;
    public Battlefield_Slot_Monobe[] b_group;
    public Battlefield_Slot_Monobe[] c_group;

    public SpriteRenderer dragged;
    public Vector3 drag_hold_offset;

    public Tilemap tilemap;
    public GameObject slot_clone;

    GCQ.Battlefield_Use battlefield_use = null;

    public GCQ.Battlefield_Use Use => battlefield_use;


    void Awake() {
        GCQ.IGame_Scope.battle_scope.battlefield_main_ref = this;

        battlefield_use = new GCQ.Battlefield_Use(this);

        tilemap.GetComponent<TilemapRenderer>().enabled = false;

        MessageBroker.Default.Receive<GCQ.Battle_Scope_Init_Complete_Trigger>().Subscribe(_ => {
            GCQ.Battle_Sys.Refresh_Spawn_Queue(GCQ.IGame_Scope.battle_scope.data);
        }).AddTo(this);

        MessageBroker.Default.Receive<GCQ.Battle_Scope_Run_Trigger>().Subscribe(_ => {
            var setting = Data_Manager.data_manager.temp_game_setting;
            GCQ.Battle_Sys.Spawn_Human_Random(GCQ.IGame_Scope.battle_scope, setting.enemy_start_num);
            GCQ.Battle_Sys.Spawn_Goblin_Random(GCQ.IGame_Scope.battle_scope, Random.Range(1, 4), setting.ally_start_num);
            GCQ.Battle_Sys.Spawn_Bed_Random(GCQ.IGame_Scope.battle_scope, Random.Range(1, 3), Data_Manager.data_manager.human_data_file.female_list[0], setting.bed_start_num);

            battlefield_use.Start_My_Update();
        }).AddTo(this);

    }
}