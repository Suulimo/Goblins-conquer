using UnityEngine;


public class Buy_Goblin_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        GCQ.Battle_Sys.Spawn_Goblin_Random(GCQ.IGame_Scope.battle_scope, Random.Range(1, 4));
    }

}
