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

    void Awake() {
        battle_scope = new Battle_Scope();
        battle_scope.Init_Scope();
        battle_state = battle_scope.state;
    }

    // Start is called before the first frame update
    void Start()
    {
        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.LastUpdate).Subscribe(_ => {
            // 關卡主要loop規則
            Loop_Sys.Run_Last_Update(Time.deltaTime);
        }).AddTo(gameObject);

    }
}
