using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class Spawn_Timer_DIsplay_Temp : MonoBehaviour
{
    public TMP_Text timer_text;

    // Start is called before the first frame update
    void Start()
    {
        Static_Game_Scope.battle_scope.state.enemy_spawn_timer.SubscribeToText(timer_text).AddTo(this);
    }

    private void OnMouseUpAsButton() {
        Static_Game_Scope.battle_scope.state.is_auto_spawn ^= true;
    }
}
