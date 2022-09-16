using TMPro;
using UnityEngine;

public class Slot_Display_Monobe : MonoBehaviour
{
    public TMP_Text rank_text;
    public Action_Bar_Monobe cd_bar;
    public GCQ.Human_Pawn human_pawn;

    public void Set_Bed_Pawn(GCQ.Human_Pawn pawn) {
        var pool_component = GetComponent<UniRx_Pool_Component>();
        var compoDispo = pool_component.GetCompositeDisposableOnReturn;

        human_pawn = pawn;
        if (human_pawn != null) {
            rank_text.text = pawn.combat.rank.ToString();
            cd_bar.Init(pawn.combat.attack_cycle, pawn.spec.combat.bed_spawn_cd);
        }
    }
}
