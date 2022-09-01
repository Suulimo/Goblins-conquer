using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Static_Game_Scope;

public static class Loop_Sys
{
    public static void Run_Last_Update(float dt) {
        for (int i = 0; i < battle_scope.state.shark_num; i++) {
            if (battle_scope.state.sharks_move_switch[i].Item1 != 0 || battle_scope.state.sharks_move_switch[i].Item2 != 0) {
            }
        }

        Battle_Sys.Run(battle_scope, dt);
    }
}

