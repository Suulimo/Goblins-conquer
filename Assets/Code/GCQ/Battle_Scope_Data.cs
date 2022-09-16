using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

[System.Serializable]
public class Battle_Scope_Data
{
    [ShowInInspector, PropertyRange(0, 3)]
    private float _play_speed {
        get => play_speed.Value;
        set => play_speed.Value = value;
    }

    [HideInInspector]
    public FloatReactiveProperty play_speed = new FloatReactiveProperty(1);

    public static float init_enemy_spawn_timer = GCQ.Game_Spec.INIT_ENEMY_SPAWN_TIME;

    public FloatReactiveProperty enemy_spawn_timer = new FloatReactiveProperty(init_enemy_spawn_timer);

    [PropertyRange(1, 100)]
    public float difficulty = 1.0f;

    public bool is_auto_spawn = true;

}
