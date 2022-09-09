using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniRx;
using static Static_Game_Scope;

public class Battle_Scene : MonoBehaviour
{
    public Battle_State battle_state = null;
    [SerializeField] Battlefield_Main_Component battle_field_main;

    void Awake() {
        battle_scope = new Battle_Scope();
        battle_scope.Init_Scope();
        battle_state = battle_scope.state;

        battle_state.play_speed.Subscribe(value =>
        {
            Time.timeScale = value;
        }).AddTo(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        var (n1, n2, n3) = battle_field_main.InitMap();
        battle_scope.Init_Scope_Slot(n1, n2, n3);

        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.LastUpdate).Subscribe(_ => {
            // 關卡主要loop規則
            Loop_Sys.Run_Last_Update(Time.deltaTime);
        }).AddTo(gameObject);

        //battle_state.growth_rate = Data_Manager.data_manager.temp_game_setting.enemy_attack_growth_rate;
    }
}
