using UnityEngine;

public class Item_Roast_Pork_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        GCQ.Static_Game_Scope.battlefield_main_ref.Use.Change_Cursor_Mode(GCQ.Battlefield_Use.Cursor_Mode.Cast);
        GCQ.Static_Game_Scope.battlefield_main_ref.Use.Change_Item_Holding(GCQ.Battlefield_Use.Item_Test.Roast_Pork);
    }
}