using UnityEngine;


public class Buy_Bed_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        GCQ.Battle_Sys.Spawn_Bed_Random(GCQ.IGame_Scope.battle_scope, Random.Range(1, 3), Data_Manager.data_manager.human_data_file.female_list[0]);
    }

}
