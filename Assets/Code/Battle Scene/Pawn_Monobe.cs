using TMPro;
using UnityEngine;

public class Pawn_Monobe : MonoBehaviour
{
    public TMP_Text rank_text;
    public TMP_Text fhp_text;
    public TMP_Text attack_text;

    public Action_Bar_Monobe hp_bar;
    public Action_Bar_Monobe cd_bar;

    public GCQ.Goblin_Pawn goblin_pawn;
    public GCQ.Human_Pawn human_pawn;
    public GCQ.Human_Pawn bed_pawn;

    // Start is called before the first frame update
    void Start() {

    }

    private void OnEnable() {
    }

    public void Set_Goblin_Pawn(GCQ.Goblin_Pawn pawn) {
        var pool_component = GetComponent<UniRx_Pool_Component>();
        var compoDispo = pool_component.GetCompositeDisposableOnReturn;

        goblin_pawn = pawn;
        if (goblin_pawn != null) {
            rank_text.text = pawn.combat.rank.ToString();
            fhp_text.text = pawn.combat.hp_max.ToString();
            attack_text.text = pawn.combat.attack_power.ToString();

            hp_bar.Init(pawn.combat.hp, pawn.combat.hp_max);
            cd_bar.Init(pawn.combat.attack_cycle, pawn.spec.combat.attack_cd);
        }
    }

    public void Set_Human_Pawn(GCQ.Human_Pawn pawn) {
        var pool_component = GetComponent<UniRx_Pool_Component>();
        var compoDispo = pool_component.GetCompositeDisposableOnReturn;

        human_pawn = pawn;
        if (human_pawn != null) {
            rank_text.text = pawn.combat.rank.ToString();
            fhp_text.text = pawn.combat.hp_max.ToString();
            attack_text.text = pawn.combat.attack_power.ToString();

            hp_bar.Init(pawn.combat.hp, pawn.combat.hp_max);
            cd_bar.Init(pawn.combat.attack_cycle, pawn.spec.combat.attack_cd);
        }
    }

    public void Set_Bed_Pawn(GCQ.Human_Pawn pawn) {
        var pool_component = GetComponent<UniRx_Pool_Component>();
        var compoDispo = pool_component.GetCompositeDisposableOnReturn;

        bed_pawn = pawn;
        if (bed_pawn != null) {
            rank_text.text = pawn.spec.rank.ToString();

            cd_bar.Init(pawn.combat.attack_cycle, pawn.spec.combat.bed_spawn_cd);
        }
    }
}