using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;
using Cysharp.Threading.Tasks;

public class Pawn_Control : MonoBehaviour
{
    public TMP_Text rank_text;
    public TMP_Text fhp_text;
    public TMP_Text attack_text;

    public Action_Bar hp_bar;
    public Action_Bar cd_bar;

    public Goblin_Pawn goblin_pawn;
    public Human_Pawn human_pawn;

    // Start is called before the first frame update
    void Start() {

    }

    private void OnEnable() {
    }

    public void Set_Goblin_Pawn(Goblin_Pawn pawn) {
        var pool_component = GetComponent<UniRx_Pool_Component>();
        var compoDispo = pool_component.GetCompositeDisposableOnReturn;

        goblin_pawn = pawn;
        if (goblin_pawn != null) {
            rank_text.text = pawn.data.rank.ToString();
            fhp_text.text = pawn.data.battle.hp[0].ToString();
            attack_text.text = pawn.data.battle.attack_power[0].ToString();

            hp_bar.Init(pawn.state.hp, pawn.data.battle.hp[0]);
            cd_bar.Init(pawn.state.attack_cycle, pawn.data.battle.attack_cd);
        }
    }

    public void Set_Human_Pawn(Human_Pawn pawn) {
        var pool_component = GetComponent<UniRx_Pool_Component>();
        var compoDispo = pool_component.GetCompositeDisposableOnReturn;

        human_pawn = pawn;
        if (human_pawn != null) {
            rank_text.text = pawn.data.rank.ToString();
            fhp_text.text = pawn.data.battle.hp[0].ToString();
            attack_text.text = pawn.data.battle.attack_power[0].ToString();

            hp_bar.Init(pawn.state.hp, pawn.data.battle.hp[0]);
            cd_bar.Init(pawn.state.attack_cycle, pawn.data.battle.attack_cd);
        }
    }

}