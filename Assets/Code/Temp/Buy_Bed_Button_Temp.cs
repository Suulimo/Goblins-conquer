using UnityEngine;


public class Buy_Bed_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        Debug.Log("Bed");
        GCQ.Battle_Sys.Spawn_Bed_Random(Random.Range(1, 3), 0, GCQ.Static_Game_Scope.battle_scope);
    }

}
