using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;

[System.Serializable]
public class Battle_State
{
    public (int, int)[] sharks_move_switch = new (int, int)[Game_Spec.MAX_PLAYER];

    public int shark_num = 1;

    [ShowInInspector, PropertyRange(0, 3)]
    private float _play_speed {
        get => play_speed.Value;
        set => play_speed.Value = value;
    }

    [HideInInspector]
    public FloatReactiveProperty play_speed = new FloatReactiveProperty(1);

    public static float init_enemy_spawn_timer = Game_Spec.INIT_ENEMY_SPAWN_TIME;

    public FloatReactiveProperty enemy_spawn_timer = new FloatReactiveProperty(init_enemy_spawn_timer);

    [PropertyRange(1, 100)]
    public float difficulty = 1.0f;

    public bool is_auto_spawn = true;

}
