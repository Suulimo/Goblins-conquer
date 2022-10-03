using UnityEngine;
using UniRx;
using DG.Tweening;

public class Item_Rage_Drug_Button_Temp : MonoBehaviour
{
    public TMPro.TMP_Text num_text;

    void Awake() {
        MessageBroker.Default.Receive<GCQ.Battle_Scope_Init_Complete_Trigger>().Subscribe(_ => {
            num_text.text = $"{GCQ.IGame_Scope.battle_scope.data.inventory_rage_drug.Value}";
            GCQ.IGame_Scope.battle_scope.data.inventory_rage_drug.Pairwise().Subscribe(pair => {
                num_text.DOCounter(pair.Previous, pair.Current, 0.8f);
            }).AddTo(this);
        }).AddTo(this);
    }

    private void OnMouseUpAsButton() {
        if (GCQ.IGame_Scope.battle_scope.data.inventory_rage_drug.Value > 0) {
            GCQ.IGame_Scope.battle_scope.battlefield_main_ref.Use.Change_Cursor_Mode(GCQ.Cursor_Mode.Cast);
            GCQ.IGame_Scope.battle_scope.battlefield_main_ref.Use.Change_Item_Holding(GCQ.Item_Test.Rage_Drug);
        }
    }
}