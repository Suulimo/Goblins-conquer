using UnityEngine;

public class Item_Rage_Drug_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        GCQ.Static_Game_Scope.battlefield_main_ref.Use.Change_Cursor_Mode(GCQ.Battlefield_Use.Cursor_Mode.Cast);
        GCQ.Static_Game_Scope.battlefield_main_ref.Use.Change_Item_Holding(GCQ.Battlefield_Use.Item_Test.Rage_Drug);
    }
}