using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Slot_Display_Component : MonoBehaviour
{
    public TMP_Text rank_text;
    public Action_Bar cd_bar;
    public Human_Pawn human_pawn;

    public void Set_Bed_Pawn(Human_Pawn pawn) {
        var pool_component = GetComponent<UniRx_Pool_Component>();
        var compoDispo = pool_component.GetCompositeDisposableOnReturn;

        human_pawn = pawn;
        if (human_pawn != null) {
            rank_text.text = pawn.state.rank.ToString();
            cd_bar.Init(pawn.state.attack_cycle, pawn.data.battle.bed_spawn_cd);
        }
    }
}
