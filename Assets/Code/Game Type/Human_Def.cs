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

// 這個是固定的資訊、數值，定義先放在本地端列表(...CatDef)，如果想放server再改
[System.Serializable]
public class Human_Other_Data
{
    public int beauty;
    public string name;
    public string sprite;
    public string sprite_bed;
    public int femininity; // 0 為男性， 大於0為女
}

[System.Serializable]
public class Human_Battle_Data
{
    public float attack_cd;
    public int attack_power;
    public string ability_id;
    public float bed_spawn_cd;
    public int hp;
}

public static class Human_Def
{
    public static Human_Data[] Default_Human_List = new Human_Data[]
    {
        new Human_Data() {
            id = "FEMALE_001",
            rank = 1,
            other = new Human_Other_Data
            {
                beauty = 1,
                name = "",
                sprite = "EnemyFemale (1)",
                sprite_bed = "Bed (1)",
                femininity = 1,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 4.9f,
                attack_power = 5,
                ability_id = "ABILITY_000",
                hp = 30,
            },
        },
        new Human_Data() {
        id = "FEMALE_002",
            rank = 2,
            other = new Human_Other_Data
            {
                beauty = 2,
                name = "",
                sprite = "EnemyFemale (2)",
                sprite_bed = "Bed (2)",
                femininity = 1,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 4.8f,
                attack_power = 10,
                ability_id = "ABILITY_000",
                hp = 60,
                bed_spawn_cd = 20,
            },
        },
        new Human_Data() {
            id = "FEMALE_003",
            rank = 3,
            other = new Human_Other_Data
            {
                beauty = 2,
                name = "HUMAN_NAME_001",
                sprite = "EnemyFemale (3)",
                sprite_bed = "Bed (3)",
                femininity = 1,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 4.7f,
                attack_power = 15,
                ability_id = "ABILITY_000",
                hp = 90,
            },
        },
        new Human_Data() {
            id = "FEMALE_004",
            rank = 4,
            other = new Human_Other_Data
            {
                beauty = 2,
                name = "HUMAN_NAME_001",
                sprite = "EnemyFemale (4)",
                sprite_bed = "Bed (4)",
                femininity = 1,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 4.6f,
                attack_power = 20,
                ability_id = "ABILITY_000",
                hp = 120,
            },
        },
    };
    public static Human_Data[] Default_Male_Human_List = new Human_Data[]
    {
        new Human_Data() {
            id = "MALE_001",
            rank = 1,
            other = new Human_Other_Data
            {
                beauty = 1,
                name = "",
                sprite = "EnemyMale (1)",
                sprite_bed = "",
                femininity = 0,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 4.9f,
                attack_power = 5,
                ability_id = "ABILITY_000",
                hp = 30,
            },
        },
        new Human_Data() {
        id = "MALE_002",
            rank = 2,
            other = new Human_Other_Data
            {
                beauty = 2,
                name = "",
                sprite = "EnemyMale (2)",
                sprite_bed = "",
                femininity = 0,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 4.8f,
                attack_power = 10,
                ability_id = "ABILITY_000",
                hp = 60,
                bed_spawn_cd = 20,
            },
        },
        new Human_Data() {
            id = "MALE_003",
            rank = 3,
            other = new Human_Other_Data
            {
                beauty = 2,
                name = "",
                sprite = "EnemyMale (3)",
                sprite_bed = "",
                femininity = 0,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 4.7f,
                attack_power = 15,
                ability_id = "ABILITY_000",
                hp = 90,
            },
        },
        new Human_Data() {
            id = "MALE_004",
            rank = 4,
            other = new Human_Other_Data
            {
                beauty = 2,
                name = "",
                sprite = "EnemyMale (4)",
                sprite_bed = "",
                femininity = 0,
            },
            battle = new Human_Battle_Data
            {
                attack_cd = 4.6f,
                attack_power = 20,
                ability_id = "ABILITY_000",
                hp = 120,
            },
        },
    };
}
