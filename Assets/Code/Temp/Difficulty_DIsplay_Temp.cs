using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class Difficulty_DIsplay_Temp : MonoBehaviour
{
    public TMP_Text timer_text;

    // Start is called before the first frame update
    void Update()
    {
        timer_text.text = Static_Game_Scope.battle_scope.state.difficulty.ToString();
    }
}
