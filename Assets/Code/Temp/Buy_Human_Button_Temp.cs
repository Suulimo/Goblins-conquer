using UnityEngine;


public class Buy_Human_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        GCQ.Battle_Sys.Spawn_Human_Random(GCQ.IGame_Scope.battle_scope);
    }

}
