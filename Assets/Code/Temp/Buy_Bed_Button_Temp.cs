using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Buy_Bed_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        Debug.Log("Bed");
        Battle_Sys.Spawn_Bed_Random(Static_Game_Scope.battle_scope);
    }

}