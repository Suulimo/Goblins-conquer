using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot_Display_Component : MonoBehaviour
{
    public Action_Bar cd_bar;
    public Human_Pawn human_pawn;

    public void Set_Bed_Pawn(Human_Pawn pawn) {
        var pool_component = GetComponent<UniRx_Pool_Component>();
        var compoDispo = pool_component.GetCompositeDisposableOnReturn;

        human_pawn = pawn;
        if (human_pawn != null) {
            cd_bar.Init(pawn.state.attack_cycle, pawn.data.battle.bed_spawn_cd);
        }
    }
}
