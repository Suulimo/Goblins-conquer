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

    [ReadOnly]
    public FloatReactiveProperty play_speed = new FloatReactiveProperty(1);

    public static float init_enemy_spawn_timer = GCQ.Game_Spec.INIT_ENEMY_SPAWN_TIME;

    public FloatReactiveProperty enemy_spawn_timer = new FloatReactiveProperty(init_enemy_spawn_timer);

    [PropertyRange(1, 100)]
    public float difficulty = 1.0f;

    public bool is_auto_spawn = true;

    [ReadOnly] public IntReactiveProperty inventory_roast_pork_num = new IntReactiveProperty(0);
    [ReadOnly] public IntReactiveProperty inventory_birth_drug = new IntReactiveProperty(0);
    [ReadOnly] public IntReactiveProperty inventory_rage_drug = new IntReactiveProperty(1);

    public ReactiveCollection<int> spawn_queue = new ReactiveCollection<int>();

}
