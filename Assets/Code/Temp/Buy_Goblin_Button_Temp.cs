using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Buy_Goblin_Button_Temp : MonoBehaviour
{
    private void OnMouseUpAsButton() {
        Debug.Log("Goblin");
        Battle_Sys.Spawn_Goblin_Random(Random.Range(1,4), Static_Game_Scope.battle_scope);
    }

}
