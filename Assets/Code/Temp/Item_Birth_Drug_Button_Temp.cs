using UnityEngine;
using UniRx;
using DG.Tweening;

public class Item_Birth_Drug_Button_Temp : MonoBehaviour
{
    public TMPro.TMP_Text num_text;

    void Awake() {
        MessageBroker.Default.Receive<GCQ.Battle_Scope_Init_Complete_Trigger>().Subscribe(_ => {
            num_text.text = $"{GCQ.Static_Game_Scope.battle_scope.data.inventory_birth_drug.Value}";
            GCQ.Static_Game_Scope.battle_scope.data.inventory_birth_drug.Pairwise().Subscribe(pair => {
                num_text.DOCounter(pair.Previous, pair.Current, 0.8f);
            }).AddTo(this);
        }).AddTo(this);
    }

    private void OnMouseUpAsButton() {
        if (GCQ.Static_Game_Scope.battle_scope.data.inventory_birth_drug.Value > 0) {
            GCQ.Static_Game_Scope.battlefield_main_ref.Use.Change_Cursor_Mode(GCQ.Battlefield_Use.Cursor_Mode.Cast);
            GCQ.Static_Game_Scope.battlefield_main_ref.Use.Change_Item_Holding(GCQ.Battlefield_Use.Item_Test.Birth_Drug);
        }
    }
}
