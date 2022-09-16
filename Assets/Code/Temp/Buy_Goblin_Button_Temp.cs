using UnityEngine;


public class Buy_Goblin_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        Debug.Log("Goblin");
        GCQ.Battle_Sys.Spawn_Goblin_Random(Random.Range(1, 4), GCQ.Static_Game_Scope.battle_scope);
    }

}
