using UnityEngine;


public class Buy_Human_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        Debug.Log("Human");
        GCQ.Battle_Sys.Spawn_Human_Random(GCQ.Static_Game_Scope.battle_scope);
    }

}
