using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Buy_Human_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        Debug.Log("Human");
        Battle_Sys.Spawn_Human_Random(Static_Game_Scope.battle_scope);
    }

}
