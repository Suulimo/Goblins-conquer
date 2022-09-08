using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Human_Data
{
    public string id;
    public int rank;

    public Human_Other_Data other;

    public Human_Battle_Data battle;

}

// �o�ӬO�T�w����T�B�ƭȡA�w�q����b���a�ݦC��(...CatDef)�A�p�G�Q��server�A��
[System.Serializable]
public class Human_Other_Data
{
    public int beauty;
    public string name;
    public string sprite;
    public string sprite_bed;
    public int femininity; // 0 ���k�ʡA �j��0���k
}

[System.Serializable]
public class Human_Battle_Data
{
    public float attack_cd;
    public int[] attack_power;
    public string ability_id;
    public int[] hp;
    public float bed_spawn_cd;
}

public static class Human_Def
{
    public static Human_Data[] Default_Human_List = new Human_Data[]
    {
        new Human_Data() {
            id = "HUMAN_001",
            rank = 1,
            other = new Human_Other_Data
            {
                beauty = 3,
                name = "HUMAN_NAME_001",
                sprite = "EnemyFemale (1)",
                sprite_bed = "ani01g",
                femininity = 1,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 5,
                attack_power = new [] { 1, 2, 3, 4, 5, },
                ability_id = "ABILITY_000",
                hp = new [] { 20, 20, 20, 30, 30 },
                bed_spawn_cd = 20,
            },
        },
        new Human_Data() {
            id = "HUMAN_002",
            rank = 2,
            other = new Human_Other_Data
            {
                beauty = 2,
                name = "HUMAN_NAME_001",
                sprite = "EnemyFemale (2)",
                sprite_bed = "ani01g",
                femininity = 1,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 5,
                attack_power = new [] { 2, 4, 6, 8, 10, },
                ability_id = "ABILITY_000",
                hp = new [] { 40, 40, 50, 60, 70 },
                bed_spawn_cd = 20,
            },
        },
        new Human_Data() {
            id = "HUMAN_003",
            rank = 3,
            other = new Human_Other_Data
            {
                beauty = 1,
                name = "HUMAN_NAME_001",
                sprite = "EnemyFemale (3)",
                sprite_bed = "ani01g",
                femininity = 1,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 5,
                attack_power = new [] { 3, 6, 9, 12, 15, },
                ability_id = "ABILITY_000",
                hp = new [] { 40, 50, 70, 70, 90 },
                bed_spawn_cd = 20,
            },
        },
    };

    public static Human_Data[] Default_Male_Human_List = new Human_Data[]
{
        new Human_Data() {
            id = "MAN_001",
            rank = 1,
            other = new Human_Other_Data
            {
                beauty = 3,
                name = "MAN_NAME_001",
                sprite = "EnemyMale (1)",
                sprite_bed = "ani01g",
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 5,
                attack_power = new [] { 1, 2, 3, 4, 5, },
                ability_id = "ABILITY_000",
                hp = new [] { 20, 20, 20, 30, 30 },
                bed_spawn_cd = 20,
            },
        },
        new Human_Data() {
            id = "" +
            "MAN_002",
            rank = 2,
            other = new Human_Other_Data
            {
                beauty = 2,
                name = "" +
                "MAN_NAME_001",
                sprite = "EnemyMale (2)",
                sprite_bed = "ani01g",
           },
            battle = new Human_Battle_Data
            {
                attack_cd = 5,
                attack_power = new [] { 2, 4, 6, 8, 10, },
                ability_id = "ABILITY_000",
                hp = new [] { 40, 40, 50, 60, 70 },
                bed_spawn_cd = 20,
            },
        },
        new Human_Data() {
            id = "" +
            "MAN_003",
            rank = 3,
            other = new Human_Other_Data
            {
                beauty = 1,
                name = "" +
                "MAN_NAME_001",
                sprite = "EnemyMale (3)",
                sprite_bed = "ani01g",
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 5,
                attack_power = new [] { 3, 6, 9, 12, 15, },
                ability_id = "ABILITY_000",
                hp = new [] { 40, 50, 70, 70, 90 },
                bed_spawn_cd = 20,
            },
        },
};

}
