using TMPro;
using UnityEngine;
using Unity.Mathematics;
using EPOOutline;

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

    Battlefield_Slot_Monobe on_slot;

    SpriteRenderer sprite_renderer;
    Color original_color;
    Collider2D my_collider2D;
    Outlinable outlinable;


    // Start is called before the first frame update
    void Awake() {
        sprite_renderer = GetComponent<SpriteRenderer>();
        original_color = sprite_renderer.color;
        my_collider2D = GetComponent<Collider2D>();
        outlinable = GetComponent<Outlinable>();
        outlinable.FrontParameters.Enabled = false;
    }

    public void Set_On_Slot(Battlefield_Slot_Monobe value) {
        on_slot = value;
    }

    public Battlefield_Slot_Monobe Get_On_Slot => on_slot;

    public void Reset_Color() {
        sprite_renderer.color = original_color;
        outlinable.FrontParameters.Enabled = false;
    }

    public void Set_Mark_Color() {
        sprite_renderer.color = Color.black * 0.5f + Color.green * 0.5f;
        outlinable.FrontParameters.Enabled = true;
    }

    public void Set_Selected_Color() {
        var c = Color.black * 0.8f + original_color * 0.2f;
        c.a = 1;
        sprite_renderer.color = c;
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