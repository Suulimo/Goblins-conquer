using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class Battle_Scene : MonoBehaviour
{
    [SerializeField] Battle_Scope_Data battle_state = null;
    [SerializeField] Battlefield_Main_Monobe battle_field_main;

    void Awake() {
        Debug.Log("Set Battle Scene");
        GCQ.IGame_Scope.battle_scope.battle_scene_ref = this;

        MessageBroker.Default.Receive<GCQ.Battle_Scope_Init_Complete_Trigger>().Subscribe(_ => {
            battle_state = GCQ.IGame_Scope.battle_scope.data;
            battle_state.difficulty += 3.0f * Data_Manager.data_manager.temp_game_setting.difficulty_growth_rate;
            battle_state.play_speed.Subscribe(value => {
                Time.timeScale = value;
            }).AddTo(this);
        }).AddTo(this);

        MessageBroker.Default.Receive<GCQ.Battle_Scope_Run_Trigger>().Subscribe(_ => {
            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.LastUpdate).Subscribe(_ => {
                // 關卡主要loop規則
                GCQ.Loop_Sys.Run_Last_Update(Time.deltaTime);
            }).AddTo(gameObject);
        }).AddTo(this);

        this.OnDestroyAsObservable().Subscribe(_ => {
            GCQ.IGame_Scope.battle_scope.battle_scene_ref = null;
            Debug.Log("Leave Battle Scene");
        });
    }
}
