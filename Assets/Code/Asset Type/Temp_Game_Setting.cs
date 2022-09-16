using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Temp Game Setting", order = 1)]
public class Temp_Game_Setting : SerializedScriptableObject
{
    public float enemy_spawn_cd = 8;
    public float difficulty_growth_rate = 0.1f;

    public float enemy_male_num_weight;
    public float enemy_female_num_weight;

    public int enemy_start_num = 3;
    public int ally_start_num = 3;
    public int bed_start_num = 1;

    public int bed_recovery_rate = 3;

    public float bed_spawn_accelerate = 1;
}