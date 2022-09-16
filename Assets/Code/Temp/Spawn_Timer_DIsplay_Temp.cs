using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UniRx;
using UnityEngine;

public class Spawn_Timer_DIsplay_Temp : MonoBehaviour
{
    public TMP_Text timer_text;

    void Awake() {
        MessageBroker.Default.Receive<GCQ.Battle_Scope_Init_Complete_Trigger>().Subscribe(_ => {
            GCQ.Static_Game_Scope.battle_scope.data.enemy_spawn_timer.SubscribeToText(timer_text).AddTo(this);
        }).AddTo(this);
    }

    private void OnMouseUpAsButton() {
        GCQ.Static_Game_Scope.battle_scope.data.is_auto_spawn ^= true;
    }
}
