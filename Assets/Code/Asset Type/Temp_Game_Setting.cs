using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "ScriptableObject/Temp Game Setting", order = 1)]
public class Temp_Game_Setting : SerializedScriptableObject
{
    public float enemy_spawn_cd = 8;

    public float enemy_male_num_weight;
    public float enemy_female_num_weight;

    public int enemy_start_num = 3;
    public int ally_start_num = 3;
    public int bed_start_num = 1;

    public float enemy_lv1_hp = 10;
    public float enemy_lv1_attack = 10;
    public float enemy_hp_growth_rate = 1.1f;
    public float enemy_attack_growth_rate = 1.05f;

    public float ally_lv1_hp = 10;
    public float ally_lv1_attack = 10;
    public float ally_hp_growth_rate = 1.1f;
    public float ally_attack_growth_rate = 1.05f;

    public int bed_recovery_rate = 3;

    public float bed_spawn_accelerate = 1;
}